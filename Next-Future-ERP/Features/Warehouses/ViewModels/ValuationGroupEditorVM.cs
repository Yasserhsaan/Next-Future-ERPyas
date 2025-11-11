using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;

namespace Next_Future_ERP.Features.Warehouses.ViewModels
{
    public partial class ValuationGroupEditorVM : ObservableObject
    {
        private readonly ValuationGroupService _service = new();
        private readonly AccountsService _accounts = new();

        public ObservableCollection<ValuationGroup> Groups { get; } = new();
        [ObservableProperty] private ValuationGroup? selectedGroup;

        // قوائم الحسابات
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

        [RelayCommand]
        public async Task LoadAsync()
        {
            Groups.Clear();
            foreach (var g in await _service.GetListAsync(CompanyId))
                Groups.Add(g);

            // تعبئة القوائم
            await LoadLookupsAsync();
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

        partial void OnSelectedGroupChanged(ValuationGroup? value)
        {
            if (value != null)
                _ = LoadAccountsAsync(value.ValuationGroupId);
        }

        private async Task LoadAccountsAsync(int vgId)
        {
            var a = await _service.GetAccountsAsync(vgId, CompanyId);
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

        [RelayCommand]
        public void NewGroup()
        {
            SelectedGroup = new ValuationGroup
            {
                CompanyId = CompanyId,
                IsActive = true
            };
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            if (SelectedGroup == null) return;

            var id = await _service.SaveAsync(SelectedGroup);
            SelectedGroup.ValuationGroupId = id;

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

            // تحديث القائمة
            await LoadAsync();
            // إعادة تحديد العنصر المحفوظ
            SelectedGroup = Groups.FirstOrDefault(g => g.ValuationGroupId == id);
        }

        [RelayCommand]
        public async Task DeleteAsync()
        {
            if (SelectedGroup == null || SelectedGroup.ValuationGroupId == 0) return;
            await _service.DeleteAsync(SelectedGroup.ValuationGroupId);
            await LoadAsync();
            SelectedGroup = null;
        }
    }
}
