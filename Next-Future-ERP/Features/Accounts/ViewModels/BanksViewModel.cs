// Features/Accounts/ViewModels/BanksViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public enum FormMode { View, New, Edit }

    public partial class BanksViewModel : ObservableObject
    {
        private readonly BankService _service;
        private readonly AccountsService _accounts = new(); // حسابات فئة "bank"

        // القوائم
        public ObservableCollection<Bank> Banks { get; } = new();
        public ObservableCollection<BankCurrencyDetail> DetailRows { get; } = new();
        public ObservableCollection<NextCurrency> CurrencyOptions { get; } = new();
        public ObservableCollection<CompanyInfoModel> CompanyOptions { get; } = new();
        public ObservableCollection<BranchModel> BranchOptions { get; } = new();
        public ObservableCollection<Account> BankAccountOptions { get; } = new();

        // ===== وضع الشاشة (يدوي) =====
        private FormMode _mode = FormMode.View;
        public FormMode Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    SaveCommand.NotifyCanExecuteChanged();
                    EditCommand.NotifyCanExecuteChanged();
                    DeleteCommand.NotifyCanExecuteChanged();
                    FirstCommand.NotifyCanExecuteChanged();
                    PrevCommand.NotifyCanExecuteChanged();
                    NextCommand.NotifyCanExecuteChanged();
                    LastCommand.NotifyCanExecuteChanged();
                    AddDetailRowCommand.NotifyCanExecuteChanged();
                    RemoveDetailRowCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(IsView));
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }
        public bool IsView => Mode == FormMode.View;
        public bool IsEditing => Mode == FormMode.New || Mode == FormMode.Edit;

        // ===== السجل الحالي (يدوي) =====
        private Bank? _selectedBank;
        public Bank? SelectedBank
        {
            get => _selectedBank;
            set
            {
                if (SetProperty(ref _selectedBank, value))
                {
                    _ = SyncSelectionsFromEntityAsync();
                    SyncDetailRowsFromEntity();
                    EditCommand.NotifyCanExecuteChanged();
                    DeleteCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private void SyncDetailRowsFromEntity()
        {
            DetailRows.Clear();
            if (SelectedBank?.CurrencyDetails == null) return;
            foreach (var d in SelectedBank.CurrencyDetails)
                DetailRows.Add(d);
        }

        // مؤشر العنصر الحالي
        private int _currentIndex = -1;
        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetProperty(ref _currentIndex, value);
        }

        // المختارات (شركة/فرع)
        private int _selectedCompanyId;
        public int SelectedCompanyId
        {
            get => _selectedCompanyId;
            set
            {
                if (SetProperty(ref _selectedCompanyId, value))
                {
                    if (SelectedBank != null) SelectedBank.CompanyId = value;
                    _branchLoadTask = LoadBranchesForCompanyAsync(value);
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
                    if (SelectedBank != null) SelectedBank.BranchId = value;
                }
            }
        }

        public BanksViewModel(BankService service) => _service = service;

        // ===== تحميل رئيسي =====
        private bool _isLoading;
        [RelayCommand]
        public async Task LoadAsync()
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                Banks.Clear();
                DetailRows.Clear();
                CurrencyOptions.Clear();
                CompanyOptions.Clear();
                BranchOptions.Clear();
                BankAccountOptions.Clear();

                foreach (var c in await _service.GetCurrenciesAsync()) CurrencyOptions.Add(c);
                foreach (var c in await _service.GetCompaniesAsync()) CompanyOptions.Add(c);
                foreach (var b in await _service.GetAllAsync()) Banks.Add(b);

                var bankAcc = await _accounts.GetByCategoryKeyAsync("bank");
                foreach (var a in bankAcc.OrderBy(x => x.AccountCode)) BankAccountOptions.Add(a);

                if (Banks.Count > 0)
                {
                    CurrentIndex = 0;
                    SelectedBank = Banks[0];
                }
                else
                {
                    SelectedBank = null;
                    CurrentIndex = -1;
                    DetailRows.Clear();
                }

                Mode = FormMode.View; // البداية في وضع العرض
            }
            finally { _isLoading = false; }
        }

        // ===== أوامر ووضعيات =====
        [RelayCommand]
        private void New()
        {
            SelectedBank = new Bank
            {
                IsActive = true,
                CreatedAt = DateTime.Now,
                CurrencyDetails = new List<BankCurrencyDetail>()
            };
            DetailRows.Clear();
            CurrentIndex = -1;
            Mode = FormMode.New;
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private void Edit()
        {
            if (SelectedBank == null || SelectedBank.BankId == 0) return;
            Mode = FormMode.Edit;
        }
        private bool CanEdit() => IsView && SelectedBank != null && SelectedBank.BankId > 0;

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            if (!CanSave() || SelectedBank == null) return;

            // تحقق أساسي
            if (SelectedBank.CompanyId <= 0 || SelectedBank.BranchId <= 0 || string.IsNullOrWhiteSpace(SelectedBank.BankName))
            {
                MessageBox.Show("رجاءً اختر الشركة والفرع وأدخل اسم البنك.", "تنبيه");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedBank.AccountNumber))
            {
                MessageBox.Show("رجاءً اختر حساب البنك من الدليل (فئة bank).", "تنبيه");
                return;
            }

            // تحقق تفاصيل العملات
            foreach (var row in DetailRows)
            {
                if (row.CurrencyId == 0)
                {
                    MessageBox.Show("رجاءً اختر العملة لكل صف.", "تنبيه");
                    return;
                }
                if (string.IsNullOrWhiteSpace(row.BankAccountNumber))
                {
                    MessageBox.Show("رجاءً أدخل رقم حساب البنك الفعلي لكل عملة.", "تنبيه");
                    return;
                }
            }
            if (DetailRows.GroupBy(x => x.CurrencyId).Any(g => g.Count() > 1))
            {
                MessageBox.Show("لا يمكن تكرار نفس العملة أكثر من مرة لنفس البنك.", "تنبيه");
                return;
            }

            // مزامنة التفاصيل
            SelectedBank.CurrencyDetails = DetailRows.ToList();

            try
            {
                if (Mode == FormMode.New)
                {
                    SelectedBank.CreatedAt = DateTime.Now;
                    SelectedBank.UpdatedAt = DateTime.Now;

                    var created = await _service.CreateAsync(SelectedBank);
                    var fresh = await _service.GetByIdAsync(created.BankId);
                    if (fresh != null)
                    {
                        Banks.Add(fresh);
                        CurrentIndex = Banks.Count - 1;
                        SelectedBank = fresh;

                        DetailRows.Clear();
                        foreach (var r in fresh.CurrencyDetails) DetailRows.Add(r);
                    }
                    MessageBox.Show("تمت الإضافة بنجاح.", "نجاح");
                }
                else // Edit
                {
                    SelectedBank.UpdatedAt = DateTime.Now;
                    await _service.UpdateAsync(SelectedBank);

                    var fresh = await _service.GetByIdAsync(SelectedBank.BankId);
                    if (fresh != null)
                    {
                        if (CurrentIndex >= 0 && CurrentIndex < Banks.Count)
                            Banks[CurrentIndex] = fresh;

                        SelectedBank = fresh;
                        DetailRows.Clear();
                        foreach (var r in fresh.CurrencyDetails) DetailRows.Add(r);
                    }
                    MessageBox.Show("تم التحديث بنجاح.", "نجاح");
                }

                Mode = FormMode.View;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ:\n{ex.Message}", "خطأ");
            }
        }
        private bool CanSave() => IsEditing;

        [RelayCommand(CanExecute = nameof(CanCancel))]
        private async Task CancelAsync()
        {
            if (Mode == FormMode.New)
            {
                if (Banks.Count > 0)
                {
                    CurrentIndex = Math.Clamp(CurrentIndex, 0, Banks.Count - 1);
                    SelectedBank = Banks.ElementAtOrDefault(CurrentIndex);
                }
                else
                {
                    SelectedBank = null;
                    DetailRows.Clear();
                }
            }
            else if (Mode == FormMode.Edit && SelectedBank != null)
            {
                var fresh = await _service.GetByIdAsync(SelectedBank.BankId);
                if (fresh != null)
                {
                    SelectedBank = fresh;
                    DetailRows.Clear();
                    foreach (var r in fresh.CurrencyDetails) DetailRows.Add(r);
                }
            }

            Mode = FormMode.View;
        }
        private bool CanCancel() => IsEditing;

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private async Task DeleteAsync()
        {
            if (SelectedBank == null || SelectedBank.BankId == 0) return;

            await _service.DeleteAsync(SelectedBank.BankId);

            if (CurrentIndex >= 0 && CurrentIndex < Banks.Count)
                Banks.RemoveAt(CurrentIndex);

            if (Banks.Count == 0)
            {
                SelectedBank = null;
                DetailRows.Clear();
                CurrentIndex = -1;
            }
            else
            {
                // ثبّت المؤشر بعد الحذف
                CurrentIndex = Math.Min(CurrentIndex, Banks.Count - 1);
                SelectedBank = Banks[CurrentIndex];
            }

            Mode = FormMode.View;
        }
        private bool CanDelete() => IsView && SelectedBank != null && SelectedBank.BankId > 0;

        // ===== التنقل — مفعّل فقط في وضع العرض =====
        [RelayCommand(CanExecute = nameof(CanNavigate))]
        private void First()
        {
            if (Banks.Count == 0) return;
            CurrentIndex = 0;
            SelectedBank = Banks[0];
        }

        [RelayCommand(CanExecute = nameof(CanNavigate))]
        private void Prev()
        {
            if (Banks.Count == 0) return;
            CurrentIndex = Math.Max(0, CurrentIndex - 1);
            SelectedBank = Banks[CurrentIndex];
        }

        [RelayCommand(CanExecute = nameof(CanNavigate))]
        private void Next()
        {
            if (Banks.Count == 0) return;
            CurrentIndex = Math.Min(Banks.Count - 1, CurrentIndex + 1);
            SelectedBank = Banks[CurrentIndex];
        }

        [RelayCommand(CanExecute = nameof(CanNavigate))]
        private void Last()
        {
            if (Banks.Count == 0) return;
            CurrentIndex = Banks.Count - 1;
            SelectedBank = Banks[CurrentIndex];
        }
        private bool CanNavigate() => IsView;

        // ===== تفاصيل العملات — أثناء التحرير فقط =====
        [RelayCommand(CanExecute = nameof(CanEditDetails))]
        private void AddDetailRow()
        {
            DetailRows.Add(new BankCurrencyDetail
            {
                AllowLimitExceed = false,
                CreatedAt = DateTime.Now
            });
        }

        [RelayCommand(CanExecute = nameof(CanEditDetails))]
        private void RemoveDetailRow(BankCurrencyDetail? row)
        {
            if (row == null) return;
            DetailRows.Remove(row);
        }
        private bool CanEditDetails() => IsEditing;

        // ===== مزامنة الشركة/الفرع مع السجل المختار + تحميل الفروع =====
        private readonly SemaphoreSlim _branchLock = new(1, 1);
        private Task _branchLoadTask = Task.CompletedTask;
        private int _lastCompanyIdLoaded = -1;

        private async Task SyncSelectionsFromEntityAsync()
        {
            if (SelectedBank == null)
            {
                SelectedCompanyId = 0;
                SelectedBranchId = 0;
                BranchOptions.Clear();
                return;
            }

            SelectedCompanyId = SelectedBank.CompanyId; // يشغّل تحميل الفروع عبر setter
            await _branchLoadTask;                      // انتظر التحميل الجاري (إن وُجد)
            SelectedBranchId = SelectedBank.BranchId;
        }

        private async Task LoadBranchesForCompanyAsync(int companyId)
        {
            await _branchLock.WaitAsync();
            try
            {
                if (companyId <= 0)
                {
                    BranchOptions.Clear();
                    _lastCompanyIdLoaded = 0;
                    return;
                }

                if (_lastCompanyIdLoaded == companyId && BranchOptions.Count > 0)
                    return;

                BranchOptions.Clear();
                var branches = await _service.GetBranchesByCompanyAsync(companyId);
                foreach (var b in branches) BranchOptions.Add(b);
                _lastCompanyIdLoaded = companyId;

                if (SelectedBranchId != 0 && !BranchOptions.Any(b => b.BranchId == SelectedBranchId))
                {
                    SelectedBranchId = 0;
                    if (SelectedBank != null) SelectedBank.BranchId = 0;
                }
                if (BranchOptions.Count > 0 && SelectedBranchId == 0)
                {
                    SelectedBranchId = BranchOptions[0].BranchId;
                    if (SelectedBank != null) SelectedBank.BranchId = SelectedBranchId;
                }
            }
            finally
            {
                _branchLock.Release();
            }
        }
    }
}
