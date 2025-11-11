using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

// enum من Models
using VType = Next_Future_ERP.Models.VoucherType;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class ReceiptVoucherViewModel : ObservableObject
    {
        private readonly ReceiptVoucherService _service;
        private readonly SemaphoreSlim _fetchLock = new(1, 1);

        // ===== أوامر إدخال/تعديل =====
        public IRelayCommand NewCommand { get; }
        public IRelayCommand EditCommand { get; }
        public IAsyncRelayCommand SaveAsyncCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand DeleteAsyncCommand { get; }
        public IAsyncRelayCommand LoadAsyncCommand { get; }
        public IAsyncRelayCommand CurrencyChangedAsyncCommand { get; }
        public IRelayCommand AddRowCommand { get; }
        public IRelayCommand<ReceiptVoucherDetail?> RemoveRowCommand { get; }

        // ===== أوامر البحث =====
        public IAsyncRelayCommand SearchAsyncCommand { get; }
        public IAsyncRelayCommand NextPageAsyncCommand { get; }
        public IAsyncRelayCommand PrevPageAsyncCommand { get; }
        public IAsyncRelayCommand<ReceiptVoucherLookupItem?> OpenSelectedCommand { get; }
        public IAsyncRelayCommand<ReceiptVoucherLookupItem?> EditSelectedCommand { get; }
        private bool _isSearchPanelExpanded = true;
        public bool IsSearchPanelExpanded
        {
            get => _isSearchPanelExpanded;
            set => SetProperty(ref _isSearchPanelExpanded, value);
        }

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

        public ObservableCollection<ReceiptVoucherDetail> DetailRows { get; } = new();

        // ===== الكيان =====
        private ReceiptVoucher? _voucher;
        public ReceiptVoucher? Voucher
        {
            get => _voucher;
            set
            {
                if (SetProperty(ref _voucher, value))
                {
                    SyncDetailRowsFromVoucher();
                    SelectedBranchId = Voucher?.BranchID ?? 0;
                    SelectedCashBoxId = Voucher?.CashBoxID ?? 0;
                    SelectedBankId = Voucher?.BankID ?? 0;
                    SelectedCurrencyId = Voucher?.CurrencyID ?? 0;
                    NotifyEnableFlags();
                }
            }
        }

        // ===== نوع الحركة (UI) =====
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

                    if (IsCash) { SelectedBankId = 0; BankOptions.Clear(); }
                    else { SelectedCashBoxId = 0; CashBoxOptions.Clear(); }

                    CurrencyOptions.Clear();
                    if (Voucher != null) { Voucher.CurrencyID = 0; Voucher.ExchangeRate = 1m; }
                    SelectedCurrencyId = 0;

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

                    SelectedCashBoxId = 0;
                    SelectedBankId = 0;
                    CashBoxOptions.Clear(); BankOptions.Clear(); CurrencyOptions.Clear();

                    _ = ReloadSourceListsForBranchAsync(value);

                    if (Voucher != null) { Voucher.CurrencyID = 0; Voucher.ExchangeRate = 1m; }
                    SelectedCurrencyId = 0;

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
                    if (IsCash) { CurrencyOptions.Clear(); _ = RefreshCurrencyOptionsAsync(); }
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
                    if (IsCheque) { CurrencyOptions.Clear(); _ = RefreshCurrencyOptionsAsync(); }
                    NotifyEnableFlags();
                }
            }
        }

        private int _selectedCurrencyId;
        public int SelectedCurrencyId
        {
            get => _selectedCurrencyId;
            set
            {
                if (SetProperty(ref _selectedCurrencyId, value))
                {
                    if (Voucher != null) Voucher.CurrencyID = value;
                }
            }
        }

        // ====== البحث / الاستعراض ======
        private int _searchBranchId;
        public int SearchBranchId
        {
            get => _searchBranchId;
            set
            {
                if (SetProperty(ref _searchBranchId, value))
                    _ = ReloadSearchSourceListsAsync();
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
        public int? SearchCashBoxId { get => _searchCashBoxId; set => SetProperty(ref _searchCashBoxId, value); }

        private int? _searchBankId;
        public int? SearchBankId { get => _searchBankId; set => SetProperty(ref _searchBankId, value); }

        private DateTime? _searchFrom;
        public DateTime? SearchFrom { get => _searchFrom; set => SetProperty(ref _searchFrom, value); }

        private DateTime? _searchTo;
        public DateTime? SearchTo { get => _searchTo; set => SetProperty(ref _searchTo, value); }

        private string? _searchDocNo;
        public string? SearchDocNo { get => _searchDocNo; set => SetProperty(ref _searchDocNo, value); }

        private string? _searchBeneficiary;
        public string? SearchBeneficiary { get => _searchBeneficiary; set => SetProperty(ref _searchBeneficiary, value); }

        public ObservableCollection<Fund> SearchCashBoxOptions { get; } = new();
        public ObservableCollection<Bank> SearchBankOptions { get; } = new();

        public ObservableCollection<ReceiptVoucherLookupItem> SearchResults { get; } = new();

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

        private ReceiptVoucherLookupItem? _selectedLookup;
        public ReceiptVoucherLookupItem? SelectedLookup
        {
            get => _selectedLookup;
            set => SetProperty(ref _selectedLookup, value);
        }

        // (اختياري) لو عندك بانلات قابلة للطي
        private bool _isResultsPanelExpanded = true;
        public bool IsResultsPanelExpanded { get => _isResultsPanelExpanded; set => SetProperty(ref _isResultsPanelExpanded, value); }

        // ===== ctor =====
        public ReceiptVoucherViewModel(ReceiptVoucherService service)
        {
            _service = service;

            // أوامر الإدخال
            NewCommand = new RelayCommand(NewImpl, () => IsView);
            EditCommand = new RelayCommand(EditImpl, () => IsView && (Voucher?.VoucherID ?? 0) > 0);
            SaveAsyncCommand = new AsyncRelayCommand(SaveAsyncImpl, () => IsEditing);
            CancelCommand = new AsyncRelayCommand(CancelImpl, () => IsEditing);
            DeleteAsyncCommand = new AsyncRelayCommand(DeleteAsyncImpl, () => IsView && (Voucher?.VoucherID ?? 0) > 0);
            LoadAsyncCommand = new AsyncRelayCommand(LoadAsyncImpl);
            CurrencyChangedAsyncCommand = new AsyncRelayCommand(CurrencyChangedAsyncImpl);
            AddRowCommand = new RelayCommand(AddRowImpl, () => IsEditing);
            RemoveRowCommand = new RelayCommand<ReceiptVoucherDetail?>(RemoveRowImpl, _ => IsEditing);

            // أوامر البحث
            SearchAsyncCommand = new AsyncRelayCommand(SearchAsyncImpl);
            NextPageAsyncCommand = new AsyncRelayCommand(NextPageAsyncImpl, () => HasNext);
            PrevPageAsyncCommand = new AsyncRelayCommand(PrevPageAsyncImpl, () => HasPrev);
            OpenSelectedCommand = new AsyncRelayCommand<ReceiptVoucherLookupItem?>(OpenSelectedAsyncImpl);
            EditSelectedCommand = new AsyncRelayCommand<ReceiptVoucherLookupItem?>(EditSelectedAsyncImpl);
        }

        private void RaiseCanExecutes()
        {
            (NewCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (EditCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (AddRowCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (RemoveRowCommand as RelayCommand<ReceiptVoucherDetail?>)?.NotifyCanExecuteChanged();
            SaveAsyncCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
            DeleteAsyncCommand.NotifyCanExecuteChanged();

            NextPageAsyncCommand.NotifyCanExecuteChanged();
            PrevPageAsyncCommand.NotifyCanExecuteChanged();
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
            IsSearchPanelExpanded = true;
            IsResultsPanelExpanded = true;


            foreach (var b in await _service.GetBranchesAsync()) BranchOptions.Add(b);
            foreach (var c in await _service.GetCostCentersAsync()) CostCenterOptions.Add(c);
            foreach (var a in await _service.GetAccountsAsync()) AccountOptions.Add(a);

            var rv = await _service.GetRVTypeAsync();
            if (rv != null) DocumentTypeOptions.Add(rv);

            // شاشة فارغة
            Voucher = null;
            SelectedBranchId = 0;
            SelectedCashBoxId = 0;
            SelectedBankId = 0;

            ResetSearchFilters();
            SearchResults.Clear();
            PageIndex = 0; TotalCount = 0;

            Mode = FormMode.View;
            RaiseCanExecutes();
        }

        private void ResetSearchFilters()
        {
            SearchBranchId = 0;
            SearchVoucherType = null;
            SearchCashBoxOptions.Clear();
            SearchBankOptions.Clear();
            SearchCashBoxId = null;
            SearchBankId = null;
            SearchFrom = null;
            SearchTo = null;
            SearchDocNo = null;
            SearchBeneficiary = null;
        }

        // ===== جديد / تعديل / حفظ / إلغاء / حذف =====
        private void NewImpl()
        {
            VoucherType = VType.Cash;

            Voucher = new ReceiptVoucher
            {
                BranchID = 0,
                VoucherType = "Cash",
                CashBoxID = null,
                BankID = null,
                CurrencyID = 0,
                ExchangeRate = 1m,
                DocumentTypeID = DocumentTypeOptions.FirstOrDefault()?.DocumentTypeId ?? 0,
                DocumentNumber = string.Empty, // يُولّد عند الحفظ
                DocumentDate = DateTime.Today,
                LocalAmount = 0m,
                ForeignAmount = null,
                Beneficiary = string.Empty,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            };

            DetailRows.Clear();
            // في القبض: نشتغل على الدائن
            DetailRows.Add(new ReceiptVoucherDetail { CrediComptCurncy = 0m, CreditCurncy = 0m });

            SelectedBranchId = 0;
            SelectedCashBoxId = 0;
            SelectedBankId = 0;

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

            // ثبّت النوع أولاً
            VoucherType = (Voucher.VoucherType == "Cheque") ? VType.Cheque : VType.Cash;

            // حمّل مصادر الفرع
            SelectedBranchId = Voucher.BranchID;
            await ReloadSourceListsForBranchAsync(SelectedBranchId);

            // أعد تعيين المصدر المختار
            SelectedCashBoxId = Voucher.CashBoxID ?? 0;
            SelectedBankId = Voucher.BankID ?? 0;

            // حمّل العملات مع الحفاظ على العملة الحالية
            await RefreshCurrencyOptionsAsync(preserveCurrentCurrency: true);

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

            // فحص مراكز التكلفة
            foreach (var d in DetailRows)
            {
                var acc = AccountOptions.FirstOrDefault(a => a.AccountId == d.AccountID);
                var requiresCC = (bool?)acc?.UsesCostCenter == true;
                if (requiresCC && (d.CostCenterID ?? 0) <= 0)
                {
                    MessageBox.Show($"الحساب ({acc?.AccountNameAr}) يتطلب مركز تكلفة.");
                    return;
                }
            }

            // القبض: نستخدم الدائن فقط ونصفّر المدين
            foreach (var r in DetailRows)
            {
                r.DebitCurncy = null;
                r.DebitCompCurncy = null;

                if ((r.CreditCurncy ?? 0) > 0 && (r.CrediComptCurncy ?? 0) == 0 && (Voucher.ExchangeRate ?? 0) > 0)
                    r.CrediComptCurncy = Math.Round((r.CreditCurncy ?? 0) * (Voucher.ExchangeRate ?? 1m), 3);
            }

            Voucher.LocalAmount = DetailRows.Sum(x => x.CrediComptCurncy ?? 0m);
            var totalForeign = DetailRows.Sum(x => x.CreditCurncy ?? 0m);
            Voucher.ForeignAmount = totalForeign == 0 ? (decimal?)null : totalForeign;

            Voucher.BranchID = SelectedBranchId;
            Voucher.CashBoxID = IsCash ? SelectedCashBoxId : null;
            Voucher.BankID = IsCheque ? SelectedBankId : null;
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
                SelectedBranchId = 0; SelectedCashBoxId = 0; SelectedBankId = 0;
                CashBoxOptions.Clear(); BankOptions.Clear(); CurrencyOptions.Clear();
            }
            else if (Mode == FormMode.Edit && Voucher != null && Voucher.VoucherID > 0)
            {
                Voucher = await _service.GetByIdAsync(Voucher.VoucherID);

                SelectedBranchId = Voucher?.BranchID ?? 0;
                SelectedCashBoxId = Voucher?.CashBoxID ?? 0;
                SelectedBankId = Voucher?.BankID ?? 0;

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
            SelectedBranchId = 0; SelectedCashBoxId = 0; SelectedBankId = 0;
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
            DetailRows.Add(new ReceiptVoucherDetail { CrediComptCurncy = 0m, CreditCurncy = 0m });
        }

        private void RemoveRowImpl(ReceiptVoucherDetail? row)
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
            await _fetchLock.WaitAsync();
            try
            {
                var current = Voucher?.CurrencyID ?? 0;

                CurrencyOptions.Clear();
                if (Voucher == null) return;

                if (IsCash && (SelectedCashBoxId ?? 0) > 0)
                {
                    foreach (var c in await _service.GetCurrenciesForCashBoxAsync(SelectedCashBoxId!.Value))
                        CurrencyOptions.Add(c);
                }
                else if (IsCheque && (SelectedBankId ?? 0) > 0)
                {
                    foreach (var c in await _service.GetCurrenciesForBankAsync(SelectedBankId!.Value))
                        CurrencyOptions.Add(c);
                }

                if (!preserveCurrentCurrency)
                {
                    if (Voucher.CurrencyID != 0 && !CurrencyOptions.Any(x => x.CurrencyId == Voucher.CurrencyID))
                    {
                        Voucher.CurrencyID = 0;
                        Voucher.ExchangeRate = 1m;
                    }
                }
                else
                {
                    if (current > 0 && CurrencyOptions.Any(x => x.CurrencyId == current))
                        Voucher.CurrencyID = current;
                }
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
        }

        private async Task NextPageAsyncImpl()
        {
            if (!HasNext) return;
            PageIndex++;
            await LoadSearchPageAsync();
        }

        private async Task PrevPageAsyncImpl()
        {
            if (!HasPrev) return;
            PageIndex--;
            await LoadSearchPageAsync();
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

        private async Task OpenSelectedAsyncImpl(ReceiptVoucherLookupItem? item)
        {
            var target = item ?? SelectedLookup;
            if (target == null) return;

            var v = await _service.GetByIdAsync(target.VoucherID);
            if (v == null) { MessageBox.Show("السند غير موجود."); return; }
          
            var tempCurrencyId = v.CurrencyID;
            var tempCashId = v.CashBoxID;
            var tempBankId = v.BankID;

            Voucher = v;
            VoucherType = (v.VoucherType == "Cheque") ? VType.Cheque : VType.Cash;

            SelectedBranchId = v.BranchID;
            await ReloadSourceListsForBranchAsync(SelectedBranchId);
            SelectedCashBoxId = tempCashId;
            SelectedBankId = tempBankId;
            SelectedCurrencyId = tempCurrencyId;

            await RefreshCurrencyOptionsAsync(preserveCurrentCurrency: true);
            if (Voucher.CurrencyID != tempCurrencyId)
            {
                SelectedCurrencyId = tempCurrencyId;
                Voucher.CurrencyID = tempCurrencyId;
                //MessageBox.Show($"🔄 تم تصحيح العملة من {Voucher.CurrencyID} إلى {tempCurrencyId}");
            }
            else
            {
                //MessageBox.Show($"✅ العملة صحيحة: {Voucher.CurrencyID}");
            }
            IsResultsPanelExpanded = false;
            Mode = FormMode.View;
            RaiseCanExecutes();
        }

        private async Task EditSelectedAsyncImpl(ReceiptVoucherLookupItem? item)
        {
            await OpenSelectedAsyncImpl(item);
            if (Voucher != null)
            {
                Mode = FormMode.Edit;
                NotifyEnableFlags();
                RaiseCanExecutes();
            }
        }
    }
}
