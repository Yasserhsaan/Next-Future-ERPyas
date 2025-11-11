using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class FundEditViewModel : ObservableObject
    {
        private readonly FundService _service;
        private readonly AccountsService _accounts = new();

        [ObservableProperty] 
        private Fund model = new();

        [ObservableProperty]
        private ObservableCollection<FundCurrencyLimit> limitRows = new();

        [ObservableProperty]
        private ObservableCollection<NextCurrency> currencyOptions = new();

        [ObservableProperty]
        private ObservableCollection<CompanyInfoModel> companyOptions = new();

        [ObservableProperty]
        private ObservableCollection<BranchModel> branchOptions = new();

        [ObservableProperty]
        private ObservableCollection<Account> accountOptions = new();

        private int _selectedCompanyId;
        public int SelectedCompanyId
        {
            get => _selectedCompanyId;
            set
            {
                if (SetProperty(ref _selectedCompanyId, value))
                {
                    if (Model != null) Model.CompanyId = value;
                    _ = LoadBranchesForCompanyAsync(value);
                }
            }
        }

        private int _selectedBranchId;
        public int SelectedBranchId
        {
            get => _selectedBranchId;
            set
            {
                if (SetProperty(ref _selectedBranchId, value))
                {
                    if (Model != null) Model.BranchId = value;
                }
            }
        }

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => Model.FundId == 0 ? "صندوق جديد" : "تعديل الصندوق";

        public FundEditViewModel(FundService service)
        {
            _service = service;
            _ = LoadOptionsAsync();
        }

        public async Task LoadOptionsAsync()
        {
            CurrencyOptions.Clear();
            CompanyOptions.Clear();
            AccountOptions.Clear();

            foreach (var c in await _service.GetCurrenciesAsync()) CurrencyOptions.Add(c);
            foreach (var c in await _service.GetCompaniesAsync()) CompanyOptions.Add(c);

            var cashAccounts = await _accounts.GetByCategoryKeyAsync("cash");
            foreach (var a in cashAccounts.OrderBy(x => x.AccountCode)) AccountOptions.Add(a);
        }

        public void InitializeNew()
        {
            Model = new Fund
            {
                IsActive = true,
                FundType = FundType.Both,
                CreatedAt = DateTime.Now,
                CurrencyLimits = new List<FundCurrencyLimit>()
            };
            LimitRows.Clear();
            SelectedCompanyId = 0;
            SelectedBranchId = 0;

            if (CompanyOptions.Count > 0)
            {
                SelectedCompanyId = CompanyOptions[0].CompId;
                Model.CompanyId = SelectedCompanyId;
            }
        }

        public async void InitializeEdit(Fund fund)
        {
            Model = Clone(fund);
            LimitRows.Clear();
            foreach (var l in Model.CurrencyLimits ?? new List<FundCurrencyLimit>())
                LimitRows.Add(l);

            SelectedCompanyId = Model.CompanyId;
            await LoadBranchesForCompanyAsync(SelectedCompanyId);
            SelectedBranchId = Model.BranchId;
        }

        private async Task LoadBranchesForCompanyAsync(int companyId)
        {
            if (companyId <= 0)
            {
                BranchOptions.Clear();
                return;
            }

            BranchOptions.Clear();
            var branches = await _service.GetBranchesByCompanyAsync(companyId);
            foreach (var b in branches) BranchOptions.Add(b);
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                // التحقق من صحة البيانات قبل الحفظ
                if (string.IsNullOrWhiteSpace(Model.FundName))
                {
                    MessageBox.Show("اسم الصندوق مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Model.CompanyId <= 0 || Model.BranchId <= 0)
                {
                    MessageBox.Show("رجاءً اختر الشركة والفرع.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Model.AccountNumber))
                {
                    MessageBox.Show("رجاءً اختر حساب الكاش للصندوق.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // تحقق تفاصيل العملات
                foreach (var row in LimitRows)
                {
                    if (row.CurrencyId == 0)
                    {
                        MessageBox.Show("رجاءً اختر العملة لكل صف.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (LimitRows.GroupBy(x => x.CurrencyId).Any(g => g.Count() > 1))
                {
                    MessageBox.Show("لا يمكن تكرار نفس العملة أكثر من مرة لنفس الصندوق.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // مزامنة التفاصيل
                Model.CurrencyLimits = LimitRows.ToList();

                if (Model.FundId == 0)
                {
                    Model.CreatedAt = DateTime.Now;
                    Model.UpdatedAt = DateTime.Now;
                    await _service.CreateAsync(Clone(Model));
                    MessageBox.Show("تم إضافة الصندوق بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Model.UpdatedAt = DateTime.Now;
                    await _service.UpdateAsync(Clone(Model));
                    MessageBox.Show("تم تحديث الصندوق بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand] 
        public void Cancel() => CloseRequested?.Invoke(this, false);

        [RelayCommand]
        private void AddLimitRow()
        {
            LimitRows.Add(new FundCurrencyLimit
            {
                AllowLimitExceed = false,
                CreatedAt = DateTime.Now
            });
        }

        [RelayCommand]
        private void RemoveLimitRow(FundCurrencyLimit? row)
        {
            if (row == null) return;
            LimitRows.Remove(row);
        }

        private static Fund Clone(Fund model) => new()
        {
            FundId = model.FundId,
            FundName = model.FundName,
            AccountNumber = model.AccountNumber,
            CompanyId = model.CompanyId,
            BranchId = model.BranchId,
            FundType = model.FundType,
            IsActive = model.IsActive,
            IsUsed = model.IsUsed,
            StopDate = model.StopDate,
            StopReason = model.StopReason,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CurrencyLimits = model.CurrencyLimits?.Select(cl => new FundCurrencyLimit
            {
                LimitId = cl.LimitId,
                FundId = cl.FundId,
                CurrencyId = cl.CurrencyId,
                MinCash = cl.MinCash,
                MaxCash = cl.MaxCash,
                MinSettlement = cl.MinSettlement,
                MaxSettlement = cl.MaxSettlement,
                AllowLimitExceed = cl.AllowLimitExceed,
                CreatedAt = cl.CreatedAt
            }).ToList() ?? new List<FundCurrencyLimit>()
        };
    }
}

