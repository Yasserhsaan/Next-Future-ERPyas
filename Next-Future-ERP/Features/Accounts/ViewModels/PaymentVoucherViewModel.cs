using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Query;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

// enum من Models
using VType = Next_Future_ERP.Models.VoucherType;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class PaymentVoucherViewModel : ObservableObject
    {
        private readonly PaymentVoucherService _service;
        private readonly SemaphoreSlim _fetchLock = new(1, 1);
        private bool _isOpeningVoucher = false; // فلاج لتجنب تداخل التحديثات أثناء فتح السند

        // ===== أوامر إدخال/تعديل =====
        public IRelayCommand NewCommand { get; }
        public IRelayCommand EditCommand { get; }
        public IAsyncRelayCommand SaveAsyncCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand DeleteAsyncCommand { get; }
        public IAsyncRelayCommand LoadAsyncCommand { get; }
        public IAsyncRelayCommand CurrencyChangedAsyncCommand { get; }
        public IRelayCommand AddRowCommand { get; }
        public IRelayCommand<PaymentVoucherDetail?> RemoveRowCommand { get; }
        public int selectedBranchIdTemp;
        public int SelectedBranchIdTemp;

        // ===== أوامر البحث =====
        public IAsyncRelayCommand SearchAsyncCommand { get; }
        public IAsyncRelayCommand NextPageAsyncCommand { get; }
        public IAsyncRelayCommand PrevPageAsyncCommand { get; }
        public IAsyncRelayCommand<PaymentVoucherLookupItem?> OpenSelectedCommand { get; }
        public IAsyncRelayCommand<PaymentVoucherLookupItem?> EditSelectedCommand { get; }
        public IRelayCommand ResetSearchCommand { get; }   // تصفير الفلاتر

        // ===== وضع الشاشة =====
        private FormMode _mode = FormMode.View;
        public FormMode Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    RaiseCanExecutes();
                    OnPropertyChanged(nameof(IsView));
                    OnPropertyChanged(nameof(IsEditing));
                    NotifyEnableFlags();
                }
            }
        }
        public bool IsView => Mode == FormMode.View;
        public bool IsEditing => Mode == FormMode.New || Mode == FormMode.Edit;

        // تمكين حقول الرأس والقوائم
        public bool IsHeaderEnabled => IsEditing;
        public bool IsCashListEnabled => IsEditing && IsCash && (SelectedBranchId > 0) && CashBoxOptions.Any();
        public bool IsBankListEnabled => IsEditing && IsCheque && (SelectedBranchId > 0) && BankOptions.Any();
        private void NotifyEnableFlags()
        {
            OnPropertyChanged(nameof(IsHeaderEnabled));
            OnPropertyChanged(nameof(IsCashListEnabled));
            OnPropertyChanged(nameof(IsBankListEnabled));
        }

        // ===== قوائم الإدخال =====
        public ObservableCollection<BranchModel> BranchOptions { get; } = new();
        public ObservableCollection<Fund> CashBoxOptions { get; } = new();
        public ObservableCollection<Bank> BankOptions { get; } = new();
        public ObservableCollection<NextCurrency> CurrencyOptions { get; } = new();
        public ObservableCollection<DocumentType> DocumentTypeOptions { get; } = new();
        public ObservableCollection<CostCenter> CostCenterOptions { get; } = new();
        public ObservableCollection<Account> AccountOptions { get; } = new();

        public ObservableCollection<PaymentVoucherDetail> DetailRows { get; } = new();

        // ===== كيان السند =====
        private PaymentVoucher? _voucher;
        public PaymentVoucher? Voucher
        {
            get => _voucher;
            set
            {
                if (SetProperty(ref _voucher, value))
                {
                    SyncDetailRowsFromVoucher();

                    SelectedBranchId = Voucher?.BranchID ?? 0;
                    SelectedCashBoxId = Voucher?.CashBoxID;
                    SelectedBankId = Voucher?.BankID;

                    NotifyEnableFlags();
                }
            }
        }

        // ===== نوع الصرف للإدخال =====
        private VType _voucherType = VType.Cash;
        public VType VoucherType
        {
            get => _voucherType;
            set
            {
                if (SetProperty(ref _voucherType, value))
                {
                    if (Voucher != null)
                        Voucher.VoucherType = (value == VType.Cheque) ? "Cheque" : "Cash";

                    if (IsCash) { SelectedBankId = null; BankOptions.Clear(); }
                    else { SelectedCashBoxId = null; CashBoxOptions.Clear(); }

                    CurrencyOptions.Clear();
                    if (Voucher != null) { Voucher.CurrencyID = 0; Voucher.ExchangeRate = 1m; }

                    _ = ReloadSourceListsForBranchAsync(SelectedBranchId);

                    OnPropertyChanged(nameof(IsCash));
                    OnPropertyChanged(nameof(IsCheque));
                    NotifyEnableFlags();
                }
            }
        }
        public bool IsCash => VoucherType == VType.Cash;
        public bool IsCheque => VoucherType == VType.Cheque;

        // ===== مختارات الإدخال =====
        private int _selectedBranchId;
        public int SelectedBranchId
        {
            get => _selectedBranchId;
            set
            {
                if (SetProperty(ref _selectedBranchId, value))
                {
                    if (Voucher != null) Voucher.BranchID = value;

                    SelectedCashBoxId = null;
                    SelectedBankId = null;

                    CashBoxOptions.Clear(); BankOptions.Clear(); CurrencyOptions.Clear();

                    _ = ReloadSourceListsForBranchAsync(value);

                    if (Voucher != null) { Voucher.CurrencyID = 0; Voucher.ExchangeRate = 1m; }

                    NotifyEnableFlags();
                }
            }
        }

        private int? _selectedCashBoxId;
        public int? SelectedCashBoxId
        {
            get => _selectedCashBoxId;
            set
            {
                if (SetProperty(ref _selectedCashBoxId, value))
                {
                    if (Voucher != null) Voucher.CashBoxID = value;
                    if (IsCash && !_isOpeningVoucher) 
                    { 
                        //MessageBox.Show("💵 SelectedCashBoxId تغير - سيتم تحديث العملات");
                        CurrencyOptions.Clear(); 
                        _ = RefreshCurrencyOptionsAsync(); 
                    }
                    else if (IsCash && _isOpeningVoucher)
                    {
                        //MessageBox.Show("🚫 SelectedCashBoxId تغير لكن تم منع تحديث العملات (أثناء فتح السند)");
                    }
                    NotifyEnableFlags();
                }
            }
        }

        private int? _selectedBankId;
        public int? SelectedBankId
        {
            get => _selectedBankId;
            set
            {
                if (SetProperty(ref _selectedBankId, value))
                {
                    if (Voucher != null) Voucher.BankID = value;
                    if (IsCheque && !_isOpeningVoucher) 
                    { 
                        //MessageBox.Show("🏦 SelectedBankId تغير - سيتم تحديث العملات");
                        CurrencyOptions.Clear(); 
                        _ = RefreshCurrencyOptionsAsync(); 
                    }
                    else if (IsCheque && _isOpeningVoucher)
                    {
                        //MessageBox.Show("🚫 SelectedBankId تغير لكن تم منع تحديث العملات (أثناء فتح السند)");
                    }
                    NotifyEnableFlags();
                }
            }
        }

        // ====== البحث / الاستعراض ======
        // فلاتر
        
        private int _searchBranchId;
        public int SearchBranchId
        {
            get => _searchBranchId;
            set
            {
                if (SetProperty(ref _searchBranchId, value))
                {
                    _ = ReloadSearchSourceListsAsync();
                }
            }
        }

        private VType? _searchVoucherType;
        public VType? SearchVoucherType
        {
            get => _searchVoucherType;
            set
            {
                if (SetProperty(ref _searchVoucherType, value))
                {
                    _ = ReloadSearchSourceListsAsync();
                    OnPropertyChanged(nameof(SearchIsCash));
                    OnPropertyChanged(nameof(SearchIsCheque));
                }
            }
        }
        public bool SearchIsCash => SearchVoucherType == VType.Cash;
        public bool SearchIsCheque => SearchVoucherType == VType.Cheque;

        private int? _searchCashBoxId;
        public int? SearchCashBoxId
        {
            get => _searchCashBoxId;
            set => SetProperty(ref _searchCashBoxId, value);
        }

        private int? _searchBankId;
        public int? SearchBankId
        {
            get => _searchBankId;
            set => SetProperty(ref _searchBankId, value);
        }
        // ====== توسيع/طي مناطق البحث والنتائج ======
        private bool _isSearchPanelExpanded = true;
        public bool IsSearchPanelExpanded
        {
            get => _isSearchPanelExpanded;
            set => SetProperty(ref _isSearchPanelExpanded, value);
        }

        private bool _isResultsPanelExpanded = false; // لا تُظهر النتائج افتراضيًا
        public bool IsResultsPanelExpanded
        {
            get => _isResultsPanelExpanded;
            set => SetProperty(ref _isResultsPanelExpanded, value);
        }
        private DateTime? _searchFrom;
        public DateTime? SearchFrom { get => _searchFrom; set => SetProperty(ref _searchFrom, value); }

        private DateTime? _searchTo;
        public DateTime? SearchTo { get => _searchTo; set => SetProperty(ref _searchTo, value); }

        private string? _searchDocNo;
        public string? SearchDocNo { get => _searchDocNo; set => SetProperty(ref _searchDocNo, value); }

        private string? _searchBeneficiary;
        public string? SearchBeneficiary { get => _searchBeneficiary; set => SetProperty(ref _searchBeneficiary, value); }

        // قوائم الفلاتر
        public ObservableCollection<Fund> SearchCashBoxOptions { get; } = new();
        public ObservableCollection<Bank> SearchBankOptions { get; } = new();

        // نتائج + صفحة
        public ObservableCollection<PaymentVoucherLookupItem> SearchResults { get; } = new();

        private int _pageSize = 20;
        public int PageSize { get => _pageSize; set => SetProperty(ref _pageSize, value); }

        private int _pageIndex;
        public int PageIndex
        {
            get => _pageIndex;
            private set
            {
                if (SetProperty(ref _pageIndex, value))
                {
                    OnPropertyChanged(nameof(HasPrev));
                    OnPropertyChanged(nameof(HasNext));
                }
            }
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            private set
            {
                if (SetProperty(ref _totalCount, value))
                {
                    OnPropertyChanged(nameof(ResultCountText));
                    OnPropertyChanged(nameof(HasPrev));
                    OnPropertyChanged(nameof(HasNext));
                }
            }
        }

        public bool HasPrev => PageIndex > 0;
        public bool HasNext => (PageIndex + 1) * PageSize < TotalCount;
        public string ResultCountText => $"نتائج: {TotalCount}";

        private PaymentVoucherLookupItem? _selectedLookup;
        public PaymentVoucherLookupItem? SelectedLookup
        {
            get => _selectedLookup;
            set => SetProperty(ref _selectedLookup, value);
        }

        // ===== ctor =====
        public PaymentVoucherViewModel(PaymentVoucherService service)
        {
            _service = service;

            // أوامر الإدخال
            NewCommand = new RelayCommand(NewImpl, () => IsView);
            EditCommand = new AsyncRelayCommand(EditAsyncImpl, () => IsView && (Voucher?.VoucherID ?? 0) > 0);
            SaveAsyncCommand = new AsyncRelayCommand(SaveAsyncImpl, () => IsEditing);
            ResetSearchCommand = new RelayCommand(ResetSearchImpl);
            CancelCommand = new AsyncRelayCommand(CancelImpl, () => IsEditing);
            DeleteAsyncCommand = new AsyncRelayCommand(DeleteAsyncImpl, () => IsView && (Voucher?.VoucherID ?? 0) > 0);
            LoadAsyncCommand = new AsyncRelayCommand(LoadAsyncImpl);
            CurrencyChangedAsyncCommand = new AsyncRelayCommand(CurrencyChangedAsyncImpl);
            AddRowCommand = new RelayCommand(AddRowImpl, () => IsEditing);
            RemoveRowCommand = new RelayCommand<PaymentVoucherDetail?>(RemoveRowImpl, _ => IsEditing);

            // أوامر البحث
            SearchAsyncCommand = new AsyncRelayCommand(SearchAsyncImpl);
            NextPageAsyncCommand = new AsyncRelayCommand(NextPageAsyncImpl, () => HasNext);
            PrevPageAsyncCommand = new AsyncRelayCommand(PrevPageAsyncImpl, () => HasPrev);
            OpenSelectedCommand = new AsyncRelayCommand<PaymentVoucherLookupItem?>(OpenSelectedAsyncImpl);
            EditSelectedCommand = new AsyncRelayCommand<PaymentVoucherLookupItem?>(EditSelectedAsyncImpl);
            ResetSearchCommand = new RelayCommand(ResetSearchImpl);
        }

        private void RaiseCanExecutes()
        {
            (NewCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (EditCommand as IRelayCommand)?.NotifyCanExecuteChanged();

            (AddRowCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (RemoveRowCommand as IRelayCommand)?.NotifyCanExecuteChanged();

            SaveAsyncCommand?.NotifyCanExecuteChanged();
            CancelCommand?.NotifyCanExecuteChanged();
            DeleteAsyncCommand?.NotifyCanExecuteChanged();

            NextPageAsyncCommand?.NotifyCanExecuteChanged();
            PrevPageAsyncCommand?.NotifyCanExecuteChanged();
        }


        // ===== تحميل القوائم الأساسية =====
        public async Task LoadAsyncImpl()
        {
            BranchOptions.Clear();
            CashBoxOptions.Clear();
            BankOptions.Clear();
            CurrencyOptions.Clear();
            DocumentTypeOptions.Clear();
            CostCenterOptions.Clear();
            AccountOptions.Clear();
            DetailRows.Clear();
            ResetSearchImpl();
            IsSearchPanelExpanded = true;   // أظهر فلاتر البحث
            IsResultsPanelExpanded = false;

            foreach (var b in await _service.GetBranchesAsync()) BranchOptions.Add(b);
            foreach (var c in await _service.GetCostCentersAsync()) CostCenterOptions.Add(c);
            foreach (var a in await _service.GetAccountsAsync()) AccountOptions.Add(a);

            var pv = await _service.GetPVTypeAsync();
            if (pv != null) DocumentTypeOptions.Add(pv);

            // شاشة فارغة
            Voucher = null;
            SelectedBranchId = 0;
            SelectedCashBoxId = null;
            SelectedBankId = null;

            // تحضير فلاتر البحث الافتراضية
            ResetSearchImpl();

            Mode = FormMode.View;
            RaiseCanExecutes();
        }

        // ===== جديد/تعديل/حفظ/إلغاء/حذف =====
        private void NewImpl()
        {
            VoucherType = VType.Cash;

            Voucher = new PaymentVoucher
            {
                BranchID = 0,
                VoucherType = "Cash",
                CashBoxID = null,
                BankID = null,
                CurrencyID = 0,
                ExchangeRate = 1m,
                DocumentTypeID = DocumentTypeOptions.FirstOrDefault()?.DocumentTypeId ?? 0,
                DocumentNumber = string.Empty,
                DocumentDate = DateTime.Today,
                LocalAmount = 0m,
                ForeignAmount = null,
                Beneficiary = string.Empty,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            };

            DetailRows.Clear();
            DetailRows.Add(new PaymentVoucherDetail { DebitCompCurncy = 0m, DebitCurncy = 0m });

            SelectedBranchId = 0;
            SelectedCashBoxId = null;
            SelectedBankId = null;

            CashBoxOptions.Clear();
            BankOptions.Clear();
            CurrencyOptions.Clear();

            Mode = FormMode.New;
            NotifyEnableFlags();
            RaiseCanExecutes();
        }

        private async void EditImpl()
        {
            if (Voucher == null || Voucher.VoucherID == 0) return;

            // 1) تأكيد نوع السند ليُحدّث IsCash/IsCheque ويضبط التمكين
            VoucherType = (Voucher.VoucherType == "Cheque") ? VType.Cheque : VType.Cash;

            // 2) إعادة بناء الاختيارات وقوائم المصدر حسب الفرع الحالي
            SelectedBranchId = Voucher.BranchID;                    // هذا وحده قد يستدعي ReloadSourceListsForBranchAsync من الـsetter
            await ReloadSourceListsForBranchAsync(SelectedBranchId); // نضمنًا

            // 3) إعادة تعيين المصدر المختار (صندوق/بنك) من السند المفتوح
            SelectedCashBoxId = Voucher.CashBoxID ?? 0;
            SelectedBankId = Voucher.BankID ?? 0;

            // 4) تحميل عملات المصدر بناءً على النوع والمصدر
            await RefreshCurrencyOptionsAsync();

            // 5) دخول وضع التحرير وتحديث تمكين الأوامر والحقول
            Mode = FormMode.Edit;
            NotifyEnableFlags();
            RaiseCanExecutes();
        }

        private async Task EditAsyncImpl()
        {
            if (Voucher == null || Voucher.VoucherID == 0) return;

            // اضبط النوع ليُحدِّث IsCash/IsCheque
            VoucherType = (Voucher.VoucherType == "Cheque") ? VType.Cheque : VType.Cash;

            // حمّل قوائم المصدر للفرع الحالي
            SelectedBranchId = Voucher.BranchID;
            await ReloadSourceListsForBranchAsync(SelectedBranchId);

            // أعد تعيين المصدر المختار
            SelectedCashBoxId = Voucher.CashBoxID;
            SelectedBankId = Voucher.BankID;

            // حمّل عملات المصدر مع الحفاظ على العملة الحالية
            await RefreshCurrencyOptionsAsync(preserveCurrentCurrency: true);

            // دخول وضع التحرير وتحديث التمكين
            Mode = FormMode.Edit;
            NotifyEnableFlags();
            RaiseCanExecutes();
        }

        private async Task SaveAsyncImpl()
        {
            if (Voucher == null) return;

            if (SelectedBranchId <= 0) { MessageBox.Show("اختر الفرع."); return; }
            if (IsCash && (SelectedCashBoxId ?? 0) <= 0) { MessageBox.Show("اختر الصندوق."); return; }
            if (IsCheque && (SelectedBankId ?? 0) <= 0) { MessageBox.Show("اختر البنك."); return; }
            if (Voucher.CurrencyID <= 0) { MessageBox.Show("اختر العملة."); return; }
            if (Voucher.DocumentTypeID <= 0) { MessageBox.Show("نوع المستند غير صحيح."); return; }
            if (DetailRows.Count == 0) { MessageBox.Show("أضف سطرًا واحدًا على الأقل."); return; }
            if (string.IsNullOrWhiteSpace(Voucher.Beneficiary)) { MessageBox.Show("أدخل اسم المستفيد."); return; }

            foreach (var d in DetailRows)
            {
                var acc = AccountOptions.FirstOrDefault(a => a.AccountId == d.AccountID);
                var requiresCC = (bool?)acc?.UsesCostCenter == true;
                if (requiresCC && (d.CostCenterID ?? 0) <= 0)
                {
                    MessageBox.Show($"الحساب ({acc?.AccountNameAr}) يتطلب مركز تكلفة."); return;
                }
            }

            foreach (var r in DetailRows)
            {
                r.CreditCurncy = null;
                r.CrediComptCurncy = null;
                if ((r.DebitCurncy ?? 0) > 0 && (r.DebitCompCurncy ?? 0) == 0 && (Voucher.ExchangeRate ?? 0) > 0)
                    r.DebitCompCurncy = Math.Round((r.DebitCurncy ?? 0) * (Voucher.ExchangeRate ?? 1m), 3);
            }

            Voucher.LocalAmount = DetailRows.Sum(x => x.DebitCompCurncy ?? 0m);
            var totalForeign = DetailRows.Sum(x => x.DebitCurncy ?? 0m);
            Voucher.ForeignAmount = totalForeign == 0 ? (decimal?)null : totalForeign;

            Voucher.BranchID = SelectedBranchId;
            Voucher.CashBoxID = IsCash ? SelectedCashBoxId : (int?)null;
            Voucher.BankID = IsCheque ? SelectedBankId : (int?)null;
            Voucher.VoucherType = IsCheque ? "Cheque" : "Cash";
            Voucher.Details = DetailRows.ToList();

            try
            {
                if (Voucher.VoucherID == 0)
                {
                    var created = await _service.CreateAsync(Voucher);
                    Voucher = await _service.GetByIdAsync(created.VoucherID);
                    MessageBox.Show("تمت الإضافة بنجاح.");
                }
                else
                {
                    await _service.UpdateAsync(Voucher);
                    Voucher = await _service.GetByIdAsync(Voucher.VoucherID);
                    MessageBox.Show("تم التحديث بنجاح.");
                }

                Mode = FormMode.View;
                NotifyEnableFlags();
                RaiseCanExecutes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الحفظ:\n{ex.Message}");
            }
        }

        private async Task CancelImpl()
        {
            if (Mode == FormMode.New)
            {
                Voucher = null; DetailRows.Clear();
                SelectedBranchId = 0; SelectedCashBoxId = null; SelectedBankId = null;
                CashBoxOptions.Clear(); BankOptions.Clear(); CurrencyOptions.Clear();
            }
            else if (Mode == FormMode.Edit && Voucher != null && Voucher.VoucherID > 0)
            {
                Voucher = await _service.GetByIdAsync(Voucher.VoucherID);
                SelectedBranchId = Voucher?.BranchID ?? 0;
                SelectedCashBoxId = Voucher?.CashBoxID;
                SelectedBankId = Voucher?.BankID;
                await ReloadSourceListsForBranchAsync(SelectedBranchId);
                await RefreshCurrencyOptionsAsync(preserveCurrentCurrency: true);
            }

            Mode = FormMode.View;
            NotifyEnableFlags();
            RaiseCanExecutes();
        }

        private async Task DeleteAsyncImpl()
        {
            if (Voucher == null || Voucher.VoucherID == 0) return;
            if (MessageBox.Show("تأكيد حذف السند؟", "تأكيد", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            await _service.DeleteAsync(Voucher.VoucherID);

            Voucher = null; DetailRows.Clear();
            SelectedBranchId = 0; SelectedCashBoxId = null; SelectedBankId = null;
            CashBoxOptions.Clear(); BankOptions.Clear(); CurrencyOptions.Clear();

            Mode = FormMode.View;
            NotifyEnableFlags();
            RaiseCanExecutes();
        }

        // ===== تغيّر العملة =====
        public async Task CurrencyChangedAsyncImpl()
        {
            if (Voucher == null || Voucher.CurrencyID <= 0) return;
            Voucher.ExchangeRate = await _service.GetExchangeRateAsync(Voucher.CurrencyID, Voucher.DocumentDate);
            OnPropertyChanged(nameof(Voucher));
        }

        // ===== تفاصيل =====
        private void AddRowImpl()
        {
            if (!IsEditing) return;
            DetailRows.Add(new PaymentVoucherDetail { DebitCompCurncy = 0m, DebitCurncy = 0m });
        }
        private void RemoveRowImpl(PaymentVoucherDetail? row)
        {
            if (!IsEditing || row == null) return;
            DetailRows.Remove(row);
        }

        // ===== Helpers (إدخال) =====
        private async Task ReloadSourceListsForBranchAsync(int branchId)
        {
            await _fetchLock.WaitAsync();
            try
            {
                CashBoxOptions.Clear();
                BankOptions.Clear();

                if (branchId > 0)
                {
                    var funds = await _service.GetCashBoxesByBranchAsync(branchId);
                    foreach (var f in funds) CashBoxOptions.Add(f);

                    var banks = await _service.GetBanksByBranchAsync(branchId);
                    foreach (var b in banks) BankOptions.Add(b);
                }

                NotifyEnableFlags();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل قوائم المصدر: {ex.Message}");
            }
            finally { _fetchLock.Release(); }
        }

        private async Task RefreshCurrencyOptionsAsync(bool preserveCurrentCurrency = false)
        {
            //MessageBox.Show($"🔄 دخول RefreshCurrencyOptionsAsync:\n" +
            //              $"preserveCurrentCurrency: {preserveCurrentCurrency}\n" +
            //              $"IsCash: {IsCash}\n" +
            //              $"IsCheque: {IsCheque}\n" +
            //              $"SelectedCashBoxId: {SelectedCashBoxId}\n" +
            //              $"SelectedBankId: {SelectedBankId}");

            await _fetchLock.WaitAsync();
            try
            {
                var current = Voucher?.CurrencyID ??0;
                //MessageBox.Show($"💰 العملة الحالية: {current}");

                CurrencyOptions.Clear();
                if (Voucher == null) 
                {
                    //MessageBox.Show("❌ Voucher is null - خروج من الدالة");
                    return;
                }

                if (IsCash && (SelectedCashBoxId ?? 0) > 0)
                {
                    //MessageBox.Show($"💵 تحميل عملات الصندوق: {SelectedCashBoxId}");
                    var currencies = await _service.GetCurrenciesForCashBoxAsync(SelectedCashBoxId!.Value);
                    //MessageBox.Show($"💵 تم العثور على {currencies.Count} عملة للصندوق");
                    foreach (var c in currencies)
                        CurrencyOptions.Add(c);
                }
                else if (IsCheque && (SelectedBankId ?? 0) > 0)
                {
                    //MessageBox.Show($"🏦 تحميل عملات البنك: {SelectedBankId}");
                    var currencies = await _service.GetCurrenciesForBankAsync(SelectedBankId!.Value);
                    //MessageBox.Show($"🏦 تم العثور على {currencies.Count} عملة للبنك");
                    foreach (var c in currencies)
                        CurrencyOptions.Add(c);
                }
                else
                {
                    //MessageBox.Show($"⚠️ لم يتم دخول أي شرط لتحميل العملات:\n" +
                    //              $"IsCash: {IsCash}, SelectedCashBoxId: {SelectedCashBoxId}\n" +
                    //              $"IsCheque: {IsCheque}, SelectedBankId: {SelectedBankId}");
                }

                //MessageBox.Show($"📊 إجمالي العملات المحملة: {CurrencyOptions.Count}");

                // منطق الحفاظ على العملة أو تصفيرها
                if (preserveCurrentCurrency && current > 0)
                {
                    //MessageBox.Show("🛡️ وضع الحفاظ على العملة");
                    // في وضع الفتح، احتفظ بالعملة الأصلية
                    if (CurrencyOptions.Any(x => x.CurrencyId == current))
                    {
                        Voucher.CurrencyID = current;
                        //MessageBox.Show($"✅ تم الحفاظ على العملة: {current}");
                    }
                    else
                    {
                        MessageBox.Show($"⚠️ العملة {current} غير متاحة في القائمة - لكن سنحتفظ بها");
                    }
                    // إذا لم تكن العملة في الخيارات، احتفظ بها على أي حال (قد تكون من مصدر مختلف)
                }
                else if (!preserveCurrentCurrency)
                {
                    //MessageBox.Show("🔄 وضع عادي - فحص صحة العملة");
                    // في وضع التعديل العادي، تحقق من وجود العملة في الخيارات
                    if (Voucher.CurrencyID != 0 && !CurrencyOptions.Any(x => x.CurrencyId == Voucher.CurrencyID))
                    {
                        //MessageBox.Show($"⚠️ العملة {Voucher.CurrencyID} غير متاحة - تصفير العملة");
                        Voucher.CurrencyID = 0;
                        Voucher.ExchangeRate = 1m;
                    }
                    else
                    {
                        //MessageBox.Show($"✅ العملة {Voucher.CurrencyID} صحيحة");
                    }
                }
                
                //MessageBox.Show($"🎯 النهاية - العملة النهائية: {Voucher.CurrencyID}");
            }
            finally { _fetchLock.Release(); }
        }


        private void SyncDetailRowsFromVoucher()
        {
            DetailRows.Clear();
            if (Voucher?.Details != null && Voucher.Details.Count > 0)
                foreach (var d in Voucher.Details) DetailRows.Add(d);
        }

        // ===== Helpers (بحث) =====
        private async Task ReloadSearchSourceListsAsync()
        {
            await _fetchLock.WaitAsync();
            try
            {
                SearchCashBoxOptions.Clear();
                SearchBankOptions.Clear();
                SearchCashBoxId = null;
                SearchBankId = null;

                if (SearchBranchId > 0)
                {
                    if (SearchIsCash || SearchVoucherType == null)
                    {
                        foreach (var f in await _service.GetCashBoxesByBranchAsync(SearchBranchId))
                            SearchCashBoxOptions.Add(f);
                    }
                    if (SearchIsCheque || SearchVoucherType == null)
                    {
                        foreach (var b in await _service.GetBanksByBranchAsync(SearchBranchId))
                            SearchBankOptions.Add(b);
                    }
                }
            }
            finally { _fetchLock.Release(); }
        }

        private async Task SearchAsyncImpl()
        {
            PageIndex = 0;
            await LoadSearchPageAsync();

            // بعد البحث: اطوِ لوحة البحث وافتح النتائج
            IsSearchPanelExpanded = false;
            IsResultsPanelExpanded = true;
        }

        private async Task NextPageAsyncImpl()
        {
            if (!HasNext) return;
            PageIndex++;
            await LoadSearchPageAsync();
            IsResultsPanelExpanded = true;
        }
        private async Task PrevPageAsyncImpl()
        {
            if (!HasPrev) return;
            PageIndex--;
            await LoadSearchPageAsync();
            IsResultsPanelExpanded = true;
        }
        private async Task LoadSearchPageAsync()
        {
            await _fetchLock.WaitAsync();
            try
            {
                SearchResults.Clear();

                string? typeStr = SearchVoucherType switch
                {
                    VType.Cash => "Cash",
                    VType.Cheque => "Cheque",
                    _ => null
                };

                int? sourceId = null;
                if (typeStr == "Cash") sourceId = (SearchCashBoxId ?? 0) > 0 ? SearchCashBoxId : null;
                if (typeStr == "Cheque") sourceId = (SearchBankId ?? 0) > 0 ? SearchBankId : null;

                var (items, total) = await _service.SearchAsync(
                    branchId: SearchBranchId > 0 ? SearchBranchId : (int?)null,
                    voucherType: typeStr,
                    sourceId: sourceId,
                    dateFrom: SearchFrom,
                    dateTo: SearchTo,
                    docNo: string.IsNullOrWhiteSpace(SearchDocNo) ? null : SearchDocNo,
                    beneficiary: string.IsNullOrWhiteSpace(SearchBeneficiary) ? null : SearchBeneficiary,
                    skip: PageIndex * PageSize,
                    take: PageSize
                );

                foreach (var it in items) SearchResults.Add(it);
                TotalCount = total;

                RaiseCanExecutes();
            }
            finally { _fetchLock.Release(); }
        }

        private async Task OpenSelectedAsyncImpl(PaymentVoucherLookupItem? item)
        {
            var target = item ?? SelectedLookup;
            if (target == null) return;

            _isOpeningVoucher = true; // تفعيل الفلاج لمنع التداخل
            //MessageBox.Show("🔍 بداية عملية الفتح");

            var v = await _service.GetByIdAsync(target.VoucherID);
            if (v == null) { 
                _isOpeningVoucher = false; // إلغاء الفلاج في حالة الخطأ
                //MessageBox.Show("السند غير موجود."); 
                return; 
            }

            //MessageBox.Show($"📁 بيانات السند من قاعدة البيانات:\n" +
            //              $"VoucherID: {v.VoucherID}\n" +
            //              $"VoucherType: {v.VoucherType}\n" +
            //              $"BranchID: {v.BranchID}\n" +
            //              $"CashBoxID: {v.CashBoxID}\n" +
            //              $"BankID: {v.BankID}\n" +
            //              $"CurrencyID: {v.CurrencyID}");
            var tempCurrencyId = v.CurrencyID;
            var tempCashId = v.CashBoxID;
            var tempBankId = v.BankID;
            VoucherType = (v.VoucherType == "Cheque") ? VType.Cheque : VType.Cash;
            // تعيين البيانات الأساسية
            Voucher = v;
            //MessageBox.Show($"✅ تم تعيين Voucher في ViewModel");
           


           
            //MessageBox.Show($"🏷️ تم تعيين VoucherType: {VoucherType}\n" +
            //              $"IsCash: {IsCash}\n" +
            //              $"IsCheque: {IsCheque}");
         
         

            // تحميل قوائم المصدر للفرع
            SelectedBranchId = v.BranchID;
          


            await ReloadSourceListsForBranchAsync(SelectedBranchId);
            //MessageBox.Show($"📋 بعد تحميل قوائم المصدر:\n" +
            //              $"عدد الصناديق: {CashBoxOptions.Count}\n" +
            //              $"عدد البنوك: {BankOptions.Count}");
           
            SelectedCashBoxId = tempCashId;
            SelectedBankId = tempBankId;
           

            //MessageBox.Show($"🏷️ تم تعيين VoucherType: {VoucherType}\n" +
            //          $"IsCash: {tempCashId}\n" + $"CurrencyID: {tempCurrencyId}\n" +
            //          $"IsCheque: {tempBankId}");


            //MessageBox.Show($"🎯 القيم المؤقتة:\n" +
            //              $"tempCashId: {tempCashId}\n" +
            //              $"tempBankId: {tempBankId}\n" +
            //              $"tempCurrencyId: {tempCurrencyId}");
            //MessageBox.Show($"🏢 تم تعيين SelectedBranchId: {SelectedBranchId}");

            // إيقاف تشغيل الأحداث مؤقتاً لتجنب التداخل


            // تعيين المصدر مباشرة (نقطة إلغاء الفلاج للسماح بالتحديث)
            _isOpeningVoucher = false; // إلغاء الفلاج هنا للسماح بالتحديث
            
            SelectedCashBoxId = tempCashId;
            SelectedBankId = tempBankId;
            
            //MessageBox.Show($"⚙️ تم تعيين المصدر:\n" +
            //              $"SelectedCashBoxId: {SelectedCashBoxId}\n" +
            //              $"SelectedBankId: {SelectedBankId}");

            // تحديث العملات مع المحافظة على العملة الأصلية
            await RefreshCurrencyOptionsAsync(preserveCurrentCurrency: true);
            
            //MessageBox.Show($"💰 بعد تحديث العملات:\n" +
            //              $"عدد العملات: {CurrencyOptions.Count}\n" +
            //              $"العملة الحالية في Voucher: {Voucher.CurrencyID}");
            
            // التأكد من تعيين العملة الصحيحة
            if (Voucher.CurrencyID != tempCurrencyId)
            {
                Voucher.CurrencyID = tempCurrencyId;
                //MessageBox.Show($"🔄 تم تصحيح العملة من {Voucher.CurrencyID} إلى {tempCurrencyId}");
            }
            else
            {
                //MessageBox.Show($"✅ العملة صحيحة: {Voucher.CurrencyID}");
            }

            // بعد الفتح: اطوِ النتائج
            IsResultsPanelExpanded = false;

            Mode = FormMode.View;
            RaiseCanExecutes();
            
            MessageBox.Show("🎉 انتهت عملية الفتح بنجاح");
        }


        private async Task EditSelectedAsyncImpl(PaymentVoucherLookupItem? item)
        {
            await OpenSelectedAsyncImpl(item);
            if (Voucher != null)
            {
                Mode = FormMode.Edit;
                RaiseCanExecutes();
            }
        }

        // ===== تصفير فلاتر ونتائج البحث =====
        private void ResetSearchImpl()
        {
            // تصفير الفلاتر
            SearchBranchId = 0;
            SearchVoucherType = null;
            SearchCashBoxId = null;
            SearchBankId = null;
            SearchFrom = null;
            SearchTo = null;
            SearchDocNo = null;
            SearchBeneficiary = null;

            // تصفير القوائم والنتائج
            SearchCashBoxOptions.Clear();
            SearchBankOptions.Clear();
            SearchResults.Clear();
            PageIndex = 0;
            TotalCount = 0;

            // ابقِ لوحة البحث مفتوحة، والنتائج مطويّة
            IsSearchPanelExpanded = true;
            IsResultsPanelExpanded = false;

            RaiseCanExecutes();
        }

    }
}
