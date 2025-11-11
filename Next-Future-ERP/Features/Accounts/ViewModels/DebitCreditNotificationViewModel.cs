// Features/Accounts/ViewModels/DebitCreditNotificationViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Models;
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
using System.Xml.Linq;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class DebitCreditNotificationViewModel : ObservableObject
    {
        private readonly DebitCreditNotificationService _service;
        private readonly SemaphoreSlim _fetchLock = new(1, 1);

        // ===== أوامر أساسية =====
        public IRelayCommand NewCommand { get; }
        public IRelayCommand EditCommand { get; }
        public IAsyncRelayCommand SaveAsyncCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand DeleteAsyncCommand { get; }
        public IAsyncRelayCommand LoadAsyncCommand { get; }
        public IRelayCommand AddRowCommand { get; }
        public IRelayCommand<DebitCreditNoteDetail?> RemoveRowCommand { get; }

        // ===== أوامر البحث =====
        public IAsyncRelayCommand SearchAsyncCommand { get; }
        public IAsyncRelayCommand NextPageAsyncCommand { get; }
        public IAsyncRelayCommand PrevPageAsyncCommand { get; }
        public IAsyncRelayCommand<DebitCreditNotificationLookupItem?> OpenSelectedCommand { get; }
        public IAsyncRelayCommand<DebitCreditNotificationLookupItem?> EditSelectedCommand { get; }

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
                    OnPropertyChanged(nameof(IsHeaderEnabled));
                }
            }
        }
        public bool IsView => Mode == FormMode.View;
        public bool IsEditing => Mode == FormMode.New || Mode == FormMode.Edit;
        public bool IsHeaderEnabled => IsEditing;

        // ===== قوائم =====
        public ObservableCollection<BranchModel> BranchOptions { get; } = new();
        public ObservableCollection<Account> AccountOptions { get; } = new();
        public ObservableCollection<NextCurrency> CurrencyOptions { get; } = new();

        // ===== الكيان =====
        private DebitCreditNotification? _notification;
        public DebitCreditNotification? Notification
        {
            get => _notification;
            set
            {
                if (SetProperty(ref _notification, value))
                {
                    SyncDetailRowsFromEntity();
                    SelectedBranchId = Notification?.BranchId ?? 0;
                    SelectedCurrencyId = Notification?.CurrencyId ?? 0;
                    SelectedAccountNumber = Notification?.AccountNumber ?? "";
                }
            }
        }

        public ObservableCollection<DebitCreditNoteDetail> DetailRows { get; } = new();

        // نوع الإشعار (لـ UI)
        private DebitCreditType _dcType = DebitCreditType.Debit;
        public DebitCreditType DCType
        {
            get => _dcType;
            set
            {
                if (SetProperty(ref _dcType, value))
                {
                    if (Notification != null)
                        Notification.NotificationType = (value == DebitCreditType.Credit) ? "C" : "D";
                }
            }
        }
        public bool IsDebit => DCType == DebitCreditType.Debit;
        public bool IsCredit => DCType == DebitCreditType.Credit;

        // مختارات الرأس
        private int _selectedBranchId;
        public int SelectedBranchId
        {
            get => _selectedBranchId;
            set
            {
                if (SetProperty(ref _selectedBranchId, value))
                {
                    if (Notification != null) Notification.BranchId = value;
                    // عيّن فرع التفاصيل الافتراضي
                    foreach (var d in DetailRows) d.BranchId = value;
                }
            }
        }

        private string _selectedAccountNumber = "";
        public string SelectedAccountNumber
        {
            get => _selectedAccountNumber;
            set
            {
                if (SetProperty(ref _selectedAccountNumber, value))
                    if (Notification != null) Notification.AccountNumber = value ?? "";
            }
        }

        private int _selectedCurrencyId;
        public int SelectedCurrencyId
        {
            get => _selectedCurrencyId;
            set
            {
                if (SetProperty(ref _selectedCurrencyId, value))
                    if (Notification != null) Notification.CurrencyId = value;
            }
        }

        // ===== البحث/الاستعراض =====
        public bool IsSearchPanelExpanded { get => _isSearchPanelExpanded; set => SetProperty(ref _isSearchPanelExpanded, value); }
        private bool _isSearchPanelExpanded = true;

        public bool IsResultsPanelExpanded { get => _isResultsPanelExpanded; set => SetProperty(ref _isResultsPanelExpanded, value); }
        private bool _isResultsPanelExpanded = true;

        // فلاتر
        public int SearchBranchId { get => _searchBranchId; set => SetProperty(ref _searchBranchId, value); }
        private int _searchBranchId;

        public DebitCreditType? SearchDCType
        {
            get => _searchDCType;
            set => SetProperty(ref _searchDCType, value);
        }
        private DebitCreditType? _searchDCType = null;

        public string? SearchAccountNumber { get => _searchAccountNumber; set => SetProperty(ref _searchAccountNumber, value); }
        private string? _searchAccountNumber;

        public DateTime? SearchFrom { get => _searchFrom; set => SetProperty(ref _searchFrom, value); }
        private DateTime? _searchFrom;

        public DateTime? SearchTo { get => _searchTo; set => SetProperty(ref _searchTo, value); }
        private DateTime? _searchTo;

        public byte? SearchStatus { get => _searchStatus; set => SetProperty(ref _searchStatus, value); }
        private byte? _searchStatus;

        public ObservableCollection<DebitCreditNotificationLookupItem> SearchResults { get; } = new();

        private int _pageSize = 20; public int PageSize { get => _pageSize; set => SetProperty(ref _pageSize, value); }
        private int _pageIndex; public int PageIndex { get => _pageIndex; private set { if (SetProperty(ref _pageIndex, value)) { OnPropertyChanged(nameof(HasPrev)); OnPropertyChanged(nameof(HasNext)); } } }
        private int _totalCount; public int TotalCount { get => _totalCount; private set { if (SetProperty(ref _totalCount, value)) OnPropertyChanged(nameof(ResultCountText)); } }

        public bool HasPrev => PageIndex > 0;
        public bool HasNext => (PageIndex + 1) * PageSize < TotalCount;
        public string ResultCountText => $"نتائج: {TotalCount}";

        private DebitCreditNotificationLookupItem? _selectedLookup;
        public DebitCreditNotificationLookupItem? SelectedLookup
        {
            get => _selectedLookup;
            set => SetProperty(ref _selectedLookup, value);
        }

        // ===== ctor =====
        public DebitCreditNotificationViewModel(DebitCreditNotificationService service)
        {
            _service = service;

            NewCommand = new RelayCommand(NewImpl, () => IsView);
            EditCommand = new RelayCommand(EditImpl, () => IsView && (Notification?.NotificationId ?? 0) > 0);
            SaveAsyncCommand = new AsyncRelayCommand(SaveAsyncImpl, () => IsEditing);
            CancelCommand = new AsyncRelayCommand(CancelImpl, () => IsEditing);
            DeleteAsyncCommand = new AsyncRelayCommand(DeleteAsyncImpl, () => IsView && (Notification?.NotificationId ?? 0) > 0);
            LoadAsyncCommand = new AsyncRelayCommand(LoadAsyncImpl);
            AddRowCommand = new RelayCommand(AddRowImpl, () => IsEditing);
            RemoveRowCommand = new RelayCommand<DebitCreditNoteDetail?>(RemoveRowImpl, _ => IsEditing);

            SearchAsyncCommand = new AsyncRelayCommand(SearchAsyncImpl);
            NextPageAsyncCommand = new AsyncRelayCommand(NextPageAsyncImpl, () => HasNext);
            PrevPageAsyncCommand = new AsyncRelayCommand(PrevPageAsyncImpl, () => HasPrev);
            OpenSelectedCommand = new AsyncRelayCommand<DebitCreditNotificationLookupItem?>(OpenSelectedAsyncImpl);
            EditSelectedCommand = new AsyncRelayCommand<DebitCreditNotificationLookupItem?>(EditSelectedAsyncImpl);
        }

        private void RaiseCanExecutes()
        {
            (NewCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (EditCommand as RelayCommand)?.NotifyCanExecuteChanged();
            SaveAsyncCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
            DeleteAsyncCommand.NotifyCanExecuteChanged();
            (AddRowCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (RemoveRowCommand as RelayCommand<DebitCreditNoteDetail?>)?.NotifyCanExecuteChanged();
            NextPageAsyncCommand.NotifyCanExecuteChanged();
            PrevPageAsyncCommand.NotifyCanExecuteChanged();
        }

        // ===== تحميل القوائم =====
        public async Task LoadAsyncImpl()
        {
            BranchOptions.Clear();
            AccountOptions.Clear();
            CurrencyOptions.Clear();
            DetailRows.Clear();

            foreach (var b in await _service.GetBranchesAsync()) BranchOptions.Add(b);
            foreach (var a in await _service.GetAccountsAsync()) AccountOptions.Add(a);
            foreach (var c in await _service.GetCurrenciesAsync()) CurrencyOptions.Add(c);

            // شاشة فارغة + إعادة فلاتر البحث
            Notification = null;
            SelectedBranchId = 0; SelectedCurrencyId = 0; SelectedAccountNumber = "";

            SearchBranchId = 0; SearchDCType = null; SearchAccountNumber = null; SearchFrom = null; SearchTo = null; SearchStatus = null;
            SearchResults.Clear(); PageIndex = 0; TotalCount = 0;
            IsSearchPanelExpanded = true; IsResultsPanelExpanded = true;

            Mode = FormMode.View;
            RaiseCanExecutes();
        }

        // ===== جديد/تعديل/حفظ/إلغاء/حذف =====
        private void NewImpl()
        {
            DCType = DebitCreditType.Debit; // افتراضي

            Notification = new DebitCreditNotification
            {
                BranchId = 0,
                NotificationType = "D",
                NotificationDate = DateTime.Today,
                PostingDate = DateTime.Today,
                AmendmentDate = null,
                CurrencyId = 0,
                AccountNumber = "",
                TotalAmount = 0m,
                CompanyId = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            };

            DetailRows.Clear();
            DetailRows.Add(new DebitCreditNoteDetail
            {
                BranchId = 0,
                PostingDate = DateTime.Today,
                Statement = "",
                AmountTransaction = 0m,
                ExchangeRate = 1m,
                AmountCompany = 0m
            });

            SelectedBranchId = 0; SelectedCurrencyId = 0; SelectedAccountNumber = "";

            Mode = FormMode.New;
            RaiseCanExecutes();
        }

        private async Task SaveAsyncImpl()
        {
            if (Notification == null) return;

            if (SelectedBranchId <= 0) { MessageBox.Show("اختر الفرع."); return; }
            if (SelectedCurrencyId <= 0) { MessageBox.Show("اختر العملة."); return; }
            if (string.IsNullOrWhiteSpace(SelectedAccountNumber)) { MessageBox.Show("أدخل رقم الحساب."); return; }
            if (DetailRows.Count == 0) { MessageBox.Show("أضف سطرًا واحدًا على الأقل."); return; }

            // حساب مبالغ الشركة إن وُجد أجنبي فقط
            foreach (var d in DetailRows)
            {
                d.BranchId = SelectedBranchId;
                if (d.AmountCompany == 0m && d.AmountTransaction != 0m && d.ExchangeRate > 0m)
                    d.AmountCompany = Math.Round(d.AmountTransaction * d.ExchangeRate, 4);
            }

            Notification.BranchId = SelectedBranchId;
            Notification.CurrencyId = SelectedCurrencyId;
            Notification.AccountNumber = SelectedAccountNumber;
            Notification.TotalAmount = DetailRows.Sum(x => x.AmountCompany);
            Notification.Details = DetailRows.ToList();

            try
            {
                if (Notification.NotificationId == 0)
                {
                    var created = await _service.CreateAsync(Notification);
                    Notification = await _service.GetByIdAsync(created.NotificationId);
                    MessageBox.Show("تمت الإضافة بنجاح.");
                }
                else
                {
                    await _service.UpdateAsync(Notification);
                    Notification = await _service.GetByIdAsync(Notification.NotificationId);
                    MessageBox.Show("تم التحديث بنجاح.");
                }

                Mode = FormMode.View;
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
                Notification = null;
                DetailRows.Clear();
                SelectedBranchId = 0; SelectedCurrencyId = 0; SelectedAccountNumber = "";
            }
            else if (Mode == FormMode.Edit && Notification != null && Notification.NotificationId > 0)
            {
                Notification = await _service.GetByIdAsync(Notification.NotificationId);
                SelectedBranchId = Notification?.BranchId ?? 0;
                SelectedCurrencyId = Notification?.CurrencyId ?? 0;
                SelectedAccountNumber = Notification?.AccountNumber ?? "";
            }
            Mode = FormMode.View;
            RaiseCanExecutes();
        }

        private async Task DeleteAsyncImpl()
        {
            if (Notification == null || Notification.NotificationId == 0) return;
            if (MessageBox.Show("تأكيد حذف الإشعار؟", "تأكيد", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            await _service.DeleteAsync(Notification.NotificationId);

            Notification = null;
            DetailRows.Clear();
            SelectedBranchId = 0; SelectedCurrencyId = 0; SelectedAccountNumber = "";

            Mode = FormMode.View;
            RaiseCanExecutes();
        }

        private void EditImpl()
        {
            if (Notification == null || Notification.NotificationId == 0) return;
            // مزامنة نوع الـUI
            DCType = (Notification.NotificationType == "C") ? DebitCreditType.Credit : DebitCreditType.Debit;
            Mode = FormMode.Edit;
            RaiseCanExecutes();
        }

        // ===== تفاصيل =====
        private void AddRowImpl()
        {
            if (!IsEditing) return;
            DetailRows.Add(new DebitCreditNoteDetail
            {
                BranchId = SelectedBranchId,
                PostingDate = Notification?.NotificationDate ?? DateTime.Today,
                Statement = "",
                AmountTransaction = 0m,
                ExchangeRate = 1m,
                AmountCompany = 0m
            });
        }

        private void RemoveRowImpl(DebitCreditNoteDetail? row)
        {
            if (!IsEditing || row == null) return;
            DetailRows.Remove(row);
        }

        private void SyncDetailRowsFromEntity()
        {
            DetailRows.Clear();
            if (Notification?.Details != null && Notification.Details.Count > 0)
                foreach (var d in Notification.Details) DetailRows.Add(d);
        }

        // ===== البحث =====
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

                string? type = SearchDCType switch
                {
                    DebitCreditType.Debit => "D",
                    DebitCreditType.Credit => "C",
                    _ => null
                };

                var (items, total) = await _service.SearchAsync(
                    branchId: SearchBranchId > 0 ? SearchBranchId : (int?)null,
                    dcType: type,
                    accountNumber: string.IsNullOrWhiteSpace(SearchAccountNumber) ? null : SearchAccountNumber,
                    dateFrom: SearchFrom,
                    dateTo: SearchTo,
                    status: SearchStatus,
                    skip: PageIndex * PageSize,
                    take: PageSize
                );

                foreach (var it in items) SearchResults.Add(it);
                TotalCount = total;

                RaiseCanExecutes();
            }
            finally { _fetchLock.Release(); }
        }

        private async Task OpenSelectedAsyncImpl(DebitCreditNotificationLookupItem? item)
        {
            var target = item ?? SelectedLookup;
            if (target == null) return;

            var n = await _service.GetByIdAsync(target.NotificationId);
            if (n == null) { MessageBox.Show("الإشعار غير موجود."); return; }

            Notification = n;
            DCType = (n.NotificationType == "C") ? DebitCreditType.Credit : DebitCreditType.Debit;

            SelectedBranchId = n.BranchId;
            SelectedCurrencyId = n.CurrencyId;
            SelectedAccountNumber = n.AccountNumber;

            // اطوِ النتائج بعد الفتح
            IsResultsPanelExpanded = false;

            Mode = FormMode.View;
            RaiseCanExecutes();
        }

        private async Task EditSelectedAsyncImpl(DebitCreditNotificationLookupItem? item)
        {
            await OpenSelectedAsyncImpl(item);
            if (Notification != null) { Mode = FormMode.Edit; RaiseCanExecutes(); }
        }
    }
}
