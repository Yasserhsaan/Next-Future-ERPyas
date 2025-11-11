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

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class DebitCreditNotificationEditViewModel : ObservableObject
    {
        private readonly DebitCreditNotificationService _service;
        private readonly SemaphoreSlim _fetchLock = new(1, 1);

        // ===== أوامر إدخال/تعديل =====
        public IAsyncRelayCommand SaveAsyncCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        public IRelayCommand AddRowCommand { get; }
        public IRelayCommand<DebitCreditNoteDetail?> RemoveRowCommand { get; }

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

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => Notification?.NotificationId == 0 ? "إشعار جديد" : $"تعديل إشعار - {Notification?.NotificationId}";

        // ===== ctor =====
        public DebitCreditNotificationEditViewModel(DebitCreditNotificationService service)
        {
            _service = service;

            SaveAsyncCommand = new AsyncRelayCommand(SaveAsyncImpl, () => IsEditing);
            CancelCommand = new AsyncRelayCommand(CancelImpl, () => IsEditing);
            AddRowCommand = new RelayCommand(AddRowImpl, () => IsEditing);
            RemoveRowCommand = new RelayCommand<DebitCreditNoteDetail?>(RemoveRowImpl, _ => IsEditing);
        }

        private void RaiseCanExecutes()
        {
            (AddRowCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (RemoveRowCommand as IRelayCommand)?.NotifyCanExecuteChanged();

            SaveAsyncCommand?.NotifyCanExecuteChanged();
            CancelCommand?.NotifyCanExecuteChanged();
        }

        // ===== تحميل القوائم =====
        public async Task LoadOptionsAsync()
        {
            BranchOptions.Clear();
            AccountOptions.Clear();
            CurrencyOptions.Clear();

            foreach (var b in await _service.GetBranchesAsync()) BranchOptions.Add(b);
            foreach (var a in await _service.GetAccountsAsync()) AccountOptions.Add(a);
            foreach (var c in await _service.GetCurrenciesAsync()) CurrencyOptions.Add(c);
        }

        // ===== جديد/تعديل/حفظ/إلغاء =====
        public void InitializeNew()
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

        public async void InitializeEdit(long notificationId)
        {
            var n = await _service.GetByIdAsync(notificationId);
            if (n == null) return;

            Notification = n;
            DCType = (n.NotificationType == "C") ? DebitCreditType.Credit : DebitCreditType.Debit;

            SelectedBranchId = n.BranchId;
            SelectedCurrencyId = n.CurrencyId;
            SelectedAccountNumber = n.AccountNumber;

            Mode = FormMode.Edit;
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

                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الحفظ:\n{ex.Message}");
            }
        }

        private async Task CancelImpl()
        {
            CloseRequested?.Invoke(this, false);
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
    }
}

