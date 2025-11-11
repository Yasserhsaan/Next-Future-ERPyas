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
    public partial class BankEditViewModel : ObservableObject
    {
        private readonly BankService _service;
        private readonly AccountsService _accounts = new();

        [ObservableProperty] 
        private Bank model = new();

        [ObservableProperty]
        private ObservableCollection<BankCurrencyDetail> detailRows = new();

        [ObservableProperty]
        private ObservableCollection<NextCurrency> currencyOptions = new();

        [ObservableProperty]
        private ObservableCollection<CompanyInfoModel> companyOptions = new();

        [ObservableProperty]
        private ObservableCollection<BranchModel> branchOptions = new();

        [ObservableProperty]
        private ObservableCollection<Account> bankAccountOptions = new();

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

        public string WindowTitle => Model.BankId == 0 ? "بنك جديد" : "تعديل البنك";

        public BankEditViewModel(BankService service)
        {
            _service = service;
            _ = LoadOptionsAsync();
        }

        public async Task LoadOptionsAsync()
        {
            CurrencyOptions.Clear();
            CompanyOptions.Clear();
            BankAccountOptions.Clear();

            foreach (var c in await _service.GetCurrenciesAsync()) CurrencyOptions.Add(c);
            foreach (var c in await _service.GetCompaniesAsync()) CompanyOptions.Add(c);

            var bankAcc = await _accounts.GetByCategoryKeyAsync("bank");
            foreach (var a in bankAcc.OrderBy(x => x.AccountCode)) BankAccountOptions.Add(a);
        }

        public void InitializeNew()
        {
            Model = new Bank
            {
                IsActive = true,
                CreatedAt = DateTime.Now,
                CurrencyDetails = new List<BankCurrencyDetail>()
            };
            DetailRows.Clear();
            SelectedCompanyId = 0;
            SelectedBranchId = 0;
        }

        public async void InitializeEdit(Bank bank)
        {
            Model = Clone(bank);
            DetailRows.Clear();
            foreach (var d in Model.CurrencyDetails ?? new List<BankCurrencyDetail>())
                DetailRows.Add(d);

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
                if (string.IsNullOrWhiteSpace(Model.BankName))
                {
                    MessageBox.Show("اسم البنك مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Model.CompanyId <= 0 || Model.BranchId <= 0)
                {
                    MessageBox.Show("رجاءً اختر الشركة والفرع.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Model.AccountNumber))
                {
                    MessageBox.Show("رجاءً اختر حساب البنك من الدليل (فئة bank).", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // تحقق تفاصيل العملات
                foreach (var row in DetailRows)
                {
                    if (row.CurrencyId == 0)
                    {
                        MessageBox.Show("رجاءً اختر العملة لكل صف.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(row.BankAccountNumber))
                    {
                        MessageBox.Show("رجاءً أدخل رقم حساب البنك الفعلي لكل عملة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (DetailRows.GroupBy(x => x.CurrencyId).Any(g => g.Count() > 1))
                {
                    MessageBox.Show("لا يمكن تكرار نفس العملة أكثر من مرة لنفس البنك.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // مزامنة التفاصيل
                Model.CurrencyDetails = DetailRows.ToList();

                if (Model.BankId == 0)
                {
                    Model.CreatedAt = DateTime.Now;
                    Model.UpdatedAt = DateTime.Now;
                    await _service.CreateAsync(Clone(Model));
                    MessageBox.Show("تم إضافة البنك بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Model.UpdatedAt = DateTime.Now;
                    await _service.UpdateAsync(Clone(Model));
                    MessageBox.Show("تم تحديث البنك بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void AddDetailRow()
        {
            DetailRows.Add(new BankCurrencyDetail
            {
                AllowLimitExceed = false,
                CreatedAt = DateTime.Now
            });
        }

        [RelayCommand]
        private void RemoveDetailRow(BankCurrencyDetail? row)
        {
            if (row == null) return;
            DetailRows.Remove(row);
        }

        private static Bank Clone(Bank model) => new()
        {
            BankId = model.BankId,
            BankName = model.BankName,
            AccountNumber = model.AccountNumber,
            CompanyId = model.CompanyId,
            BranchId = model.BranchId,
            IsActive = model.IsActive,
            StopDate = model.StopDate,
            StopReason = model.StopReason,
            ContactInfo = model.ContactInfo,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CurrencyDetails = model.CurrencyDetails?.Select(cd => new BankCurrencyDetail
            {
                DetailId = cd.DetailId,
                BankId = cd.BankId,
                CurrencyId = cd.CurrencyId,
                BankAccountNumber = cd.BankAccountNumber,
                MinCash = cd.MinCash,
                MaxCash = cd.MaxCash,
                MinTransaction = cd.MinTransaction,
                MaxTransaction = cd.MaxTransaction,
                AllowLimitExceed = cd.AllowLimitExceed,
                CreatedAt = cd.CreatedAt
            }).ToList() ?? new List<BankCurrencyDetail>()
        };
    }
}

