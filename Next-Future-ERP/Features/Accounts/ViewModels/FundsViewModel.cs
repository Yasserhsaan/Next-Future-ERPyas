using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Next_Future_ERP.Features.Accounts.Services;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class FundsViewModel : ObservableObject
    {
        private readonly FundService _service;
      
        public ObservableCollection<FundCurrencyLimit> LimitRows { get; } = new();
        private readonly AccountsService _accounts = new();
        public ObservableCollection<Account> AccountOptions { get; } = new();

        // القوائم
        public ObservableCollection<Fund> Funds { get; } = new();
        public ObservableCollection<NextCurrency> CurrencyOptions { get; } = new();
        public ObservableCollection<CompanyInfoModel> CompanyOptions { get; } = new();
        public ObservableCollection<BranchModel> BranchOptions { get; } = new();

        // السجل الحالي
        private Fund? _selectedFund;
        public Fund? SelectedFund
        {
            get => _selectedFund;
            set
            {
                if (SetProperty(ref _selectedFund, value))
                {
                    _ = SyncSelectionsFromFundAsync(); // الشركة/الفرع
                    SyncLimitRowsFromFund();           // ← جديدة: زامِن حدود العملات
                }
            }
        }

        private void SyncLimitRowsFromFund()
        {
            LimitRows.Clear();
            if (SelectedFund?.CurrencyLimits == null) return;
            foreach (var l in SelectedFund.CurrencyLimits)
                LimitRows.Add(l);
        }


        private int _currentIndex = -1;
        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetProperty(ref _currentIndex, value);
        }

        // المختارات
        private int _selectedCompanyId;
        public int SelectedCompanyId
        {
            get => _selectedCompanyId;
            set
            {
                if (SetProperty(ref _selectedCompanyId, value))
                {
                    if (SelectedFund != null) SelectedFund.CompanyId = value;
                    // أطلق مهمة التحميل ولا تنشئ مهمة ثانية لو السابقة لم تنتهِ
                    _branchLoadTask = LoadBranchesForCompanyAsync(value);
                }
            }
        }
        private async Task LoadCashAccountsAsync()
        {
            AccountOptions.Clear();
            var cashAccounts = await _accounts.GetByCategoryKeyAsync("cash"); // ⬅️ فقط الكاش
            foreach (var a in cashAccounts.OrderBy(x => x.AccountCode))
                AccountOptions.Add(a);
        }


        private int _selectedBranchId;
        public int SelectedBranchId
        {
            get => _selectedBranchId;
            set
            {
                if (SetProperty(ref _selectedBranchId, value))
                {
                    if (SelectedFund != null) SelectedFund.BranchId = value;
                }
            }
        }

        public FundsViewModel(FundService service)
        {
            _service = service;
            // ⚠️ لا تنادِ LoadAsync هنا، سنناديه من الصفحة مرة واحدة
        }

        // ===== تحميل رئيسي =====
        private bool _isLoading;
        [RelayCommand]
        public async Task LoadAsync()
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                Funds.Clear();
                CurrencyOptions.Clear();
                CompanyOptions.Clear();
                BranchOptions.Clear();

                foreach (var c in await _service.GetCurrenciesAsync()) CurrencyOptions.Add(c);
                foreach (var c in await _service.GetCompaniesAsync()) CompanyOptions.Add(c);
                foreach (var f in await _service.GetAllAsync()) Funds.Add(f);
                await LoadCashAccountsAsync(); // ⬅️ هنا

                if (Funds.Count > 0)
                {
                    CurrentIndex = 0;
                    SelectedFund = Funds[0]; // سيستدعي المزامنة مع الشركة/الفرع
                }
                else
                {
                    NewInternal();
                }
            }
            finally { _isLoading = false; }
        }

        // ===== أوامر CRUD =====
        [RelayCommand] private void New() => NewInternal();

        [RelayCommand]


        private async Task SaveAsync()
        {
            if (SelectedFund == null)
                return;

            if (SelectedFund.CompanyId <= 0 || SelectedFund.BranchId <= 0 || string.IsNullOrWhiteSpace(SelectedFund.FundName))
            {
                System.Windows.MessageBox.Show("رجاءً اختر الشركة والفرع وأدخل اسم الصندوق.", "تنبيه");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedFund.AccountNumber))
            {
                System.Windows.MessageBox.Show("رجاءً اختر حساب الكاش للصندوق.", "تنبيه");
                return;
            }


            // 1) مزامنة جدول العرض مع الكيان (ينطبق على الإضافة والتعديل)
            SelectedFund.CurrencyLimits = LimitRows.ToList();

            try
            {
                if (SelectedFund.FundId == 0)
                {
                    // إضافة
                    SelectedFund.CreatedAt = DateTime.Now;
                    SelectedFund.UpdatedAt = DateTime.Now;

                    var created = await _service.CreateAsync(SelectedFund);

                    // إعادة تحميل السجل لضمان جلب LimitId/العلاقات
                    var fresh = await _service.GetByIdAsync(created.FundId);
                    if (fresh != null)
                    {
                        Funds.Add(fresh);
                        CurrentIndex = Funds.Count - 1;
                        SelectedFund = fresh;
                        // حدّث جدول العرض
                        LimitRows.Clear();
                        foreach (var r in fresh.CurrencyLimits) LimitRows.Add(r);
                    }

                    System.Windows.MessageBox.Show("تمت الإضافة بنجاح.", "نجاح");
                }
                else
                {
                    // تعديل
                    SelectedFund.UpdatedAt = DateTime.Now;

                    await _service.UpdateAsync(SelectedFund);

                    // إعادة تحميل بعد التحديث لتحديث LimitId/العلاقات
                    var fresh = await _service.GetByIdAsync(SelectedFund.FundId);
                    if (fresh != null)
                    {
                        if (CurrentIndex >= 0 && CurrentIndex < Funds.Count)
                            Funds[CurrentIndex] = fresh;

                        SelectedFund = fresh;
                        LimitRows.Clear();
                        foreach (var r in fresh.CurrencyLimits) LimitRows.Add(r);
                    }

                    System.Windows.MessageBox.Show("تم التحديث بنجاح.", "نجاح");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"حدث خطأ أثناء الحفظ:\n{ex.Message}", "خطأ");
            }
        }


        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (SelectedFund == null || SelectedFund.FundId == 0) return;

            await _service.DeleteAsync(SelectedFund.FundId);

            if (CurrentIndex >= 0 && CurrentIndex < Funds.Count)
                Funds.RemoveAt(CurrentIndex);

            if (Funds.Count == 0) NewInternal();
            else
            {
                CurrentIndex = Math.Min(CurrentIndex, Funds.Count - 1);
                SelectedFund = Funds[CurrentIndex];
            }
        }

        [RelayCommand] private void First() { if (Funds.Count == 0) return; CurrentIndex = 0; SelectedFund = Funds[0]; }
        [RelayCommand] private void Prev() { if (Funds.Count == 0) return; CurrentIndex = Math.Max(0, CurrentIndex - 1); SelectedFund = Funds[CurrentIndex]; }
        [RelayCommand] private void Next() { if (Funds.Count == 0) return; CurrentIndex = Math.Min(Funds.Count - 1, CurrentIndex + 1); SelectedFund = Funds[CurrentIndex]; }
        [RelayCommand] private void Last() { if (Funds.Count == 0) return; CurrentIndex = Funds.Count - 1; SelectedFund = Funds[CurrentIndex]; }

        // حدود العملات
        [RelayCommand]
        private void AddLimitRow()
        {
            var row = new FundCurrencyLimit
            {
                AllowLimitExceed = false,
                CreatedAt = DateTime.Now
            };
            LimitRows.Add(row);
        }

        [RelayCommand]
        private void RemoveLimitRow(FundCurrencyLimit? row)
        {
            if (row == null) return;
            LimitRows.Remove(row);
        }

        // ===== مساعدات =====
        private void NewInternal()
        {
            SelectedFund = new Fund
            {
                IsActive = true,
                FundType = FundType.Both,
                CreatedAt = DateTime.Now,
                CurrencyLimits = new List<FundCurrencyLimit>()
            };

            // اختر أول شركة إن وُجدت ثم حمل الفروع
            if (CompanyOptions.Count > 0)
            {
                SelectedCompanyId = CompanyOptions[0].CompId;   // يشغّل LoadBranchesForCompanyAsync عبر الـ setter
                SelectedFund.CompanyId = SelectedCompanyId;
            }
            else
            {
                SelectedCompanyId = 0;
            }

            SelectedBranchId = 0; // سيُملأ بعد تحميل الفروع إن أردت تعيين أول فرع يدويًا

            CurrentIndex = -1;
            SyncLimitRowsFromFund(); // يفرّغ LimitRows بما أن القائمة فاضية الآن
        }



        private async Task SyncSelectionsFromFundAsync()
        {
            if (SelectedFund == null)
            {
                SelectedCompanyId = 0;
                SelectedBranchId = 0;
                BranchOptions.Clear();
                return;
            }

            SelectedCompanyId = SelectedFund.CompanyId; // يطلق _branchLoadTask
            await _branchLoadTask;                      // ننتظر نفس المهمة بدل تشغيل أخرى
            SelectedBranchId = SelectedFund.BranchId;
        }

        // تحميل الفروع — مؤمَّن ضد التوازي
        private readonly SemaphoreSlim _branchLock = new(1, 1);
        private Task _branchLoadTask = Task.CompletedTask;
        private int _lastCompanyIdLoaded = -1;

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

                // إذا نفس الشركة سبق وتحملت وفروعها موجودة لا تعِد التحميل
                if (_lastCompanyIdLoaded == companyId && BranchOptions.Count > 0)
                    return;

                BranchOptions.Clear();
                var branches = await _service.GetBranchesByCompanyAsync(companyId);
                foreach (var b in branches) BranchOptions.Add(b);
                _lastCompanyIdLoaded = companyId;

                // لو الفرع المحدد غير ضمن فروع هذه الشركة
                if (SelectedBranchId != 0 && !BranchOptions.Any(b => b.BranchId == SelectedBranchId))
                {
                    SelectedBranchId = 0;
                    if (SelectedFund != null) SelectedFund.BranchId = 0;
                }
                if (BranchOptions.Count > 0 && SelectedBranchId == 0)
                {
                    SelectedBranchId = BranchOptions[0].BranchId;
                    if (SelectedFund != null) SelectedFund.BranchId = SelectedBranchId;
                }

            }
            finally
            {
                _branchLock.Release();
            }
        }
    }
}
