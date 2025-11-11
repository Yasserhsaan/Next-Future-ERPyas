using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using Next_Future_ERP.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Warehouses.ViewModels
{
    public partial class ValuationGroupEditViewModel : ObservableObject
    {
        private readonly IValuationGroupService _service;
        private readonly AccountsService _accounts;

        [ObservableProperty] private ValuationGroup model;
        public ObservableCollection<Account> InventoryList { get; } = new();
        public ObservableCollection<Account> CogsList { get; } = new();
        public ObservableCollection<Account> SalesList { get; } = new();
        public ObservableCollection<Account> SalesDiscountList { get; } = new();
        public ObservableCollection<Account> LossList { get; } = new();
        public ObservableCollection<Account> AdjustmentList { get; } = new();
        public ObservableCollection<Account> EarnedDiscountList { get; } = new();
        public ObservableCollection<Account> ExpenseList { get; } = new();
        public ObservableCollection<Account> TaxPurchaseList { get; } = new();

        public ValuationGroupAccountsVM Accounts { get; } = new();
        public int CompanyId { get; set; } = 1;

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => Model.ValuationGroupId == 0 ? "مجموعة تقييم جديدة" : "تعديل مجموعة التقييم";

        public ValuationGroupEditViewModel(IValuationGroupService service, AccountsService accounts, ValuationGroup model)
        {
            _service = service;
            _accounts = accounts;
            Model = Clone(model);
            _ = LoadLookupsAsync();
            _ = LoadAccountsAsync();
        }

        private async Task LoadLookupsAsync()
        {
            async Task fill(ObservableCollection<Account> coll, string key)
            {
                coll.Clear();
                var list = await _accounts.GetByCategoryKeyAsync(key);
                foreach (var a in list) coll.Add(a);
            }

            await fill(InventoryList, "inventory");
            await fill(CogsList, "cogs");
            await fill(SalesList, "sales");
            await fill(SalesDiscountList, "selling_expense");
            await fill(LossList, "other_expense");
            await fill(AdjustmentList, "cogs");
            await fill(EarnedDiscountList, "other_income");
            await fill(ExpenseList, "admin_expense");
            await fill(TaxPurchaseList, "other_receivable");
        }

        private async Task LoadAccountsAsync()
        {
            if (Model.ValuationGroupId > 0)
            {
                var a = await _service.GetAccountsAsync(Model.ValuationGroupId, CompanyId);
                Accounts.InventoryAcc = a.InventoryAcc;
                Accounts.CogsAcc = a.COGSAcc;
                Accounts.SalesAcc = a.SalesAcc;
                Accounts.SalesDiscountAcc = a.SalesDiscountAcc;
                Accounts.LossAcc = a.LossAcc;
                Accounts.AdjustmentAcc = a.AdjustmentAcc;
                Accounts.EarnedDiscountAccount = a.EarnedDiscountAccount;
                Accounts.ExpenseAcc = a.ExpenseAcc;
                Accounts.TaxAccPurchase = a.TaxAccPurchase;
            }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Model.ValuationGroupCode))
                {
                    MessageBox.Show("كود مجموعة التقييم مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(Model.ValuationGroupName))
                {
                    MessageBox.Show("اسم مجموعة التقييم مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var id = await _service.SaveAsync(Clone(Model));
                Model.ValuationGroupId = id;

                // Upsert للحسابات على نفس ValuationGroup و Company
                var accRow = new ValuationGroupAccount
                {
                    ValuationGroup = id,
                    CompanyId = CompanyId,
                    InventoryAcc = Accounts.InventoryAcc,
                    COGSAcc = Accounts.CogsAcc,
                    SalesAcc = Accounts.SalesAcc,
                    SalesDiscountAcc = Accounts.SalesDiscountAcc,
                    LossAcc = Accounts.LossAcc,
                    AdjustmentAcc = Accounts.AdjustmentAcc,
                    EarnedDiscountAccount = Accounts.EarnedDiscountAccount,
                    ExpenseAcc = Accounts.ExpenseAcc,
                    TaxAccPurchase = Accounts.TaxAccPurchase
                };
                await _service.UpsertAccountsAsync(accRow);

                MessageBox.Show("تم حفظ مجموعة التقييم بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand] 
        public void Cancel() => CloseRequested?.Invoke(this, false);

        private static ValuationGroup Clone(ValuationGroup model) => new()
        {
            ValuationGroupId = model.ValuationGroupId,
            CompanyId = model.CompanyId,
            ValuationGroupCode = model.ValuationGroupCode,
            ValuationGroupName = model.ValuationGroupName,
            Description = model.Description,
            IsActive = model.IsActive,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate,
            CreatedBy = model.CreatedBy,
            ModifiedBy = model.ModifiedBy,
            CostCenterId = model.CostCenterId
        };
    }
}
