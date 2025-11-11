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

// enum من Models
using VType = Next_Future_ERP.Models.VoucherType;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class PaymentVoucherEditViewModel : ObservableObject
    {
        private readonly PaymentVoucherService _service;
        private readonly SemaphoreSlim _fetchLock = new(1, 1);
        private bool _isOpeningVoucher = false; // فلاج لتجنب تداخل التحديثات أثناء فتح السند

        // ===== أوامر إدخال/تعديل =====
        public IAsyncRelayCommand SaveAsyncCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand CurrencyChangedAsyncCommand { get; }
        public IRelayCommand AddRowCommand { get; }
        public IRelayCommand<PaymentVoucherDetail?> RemoveRowCommand { get; }

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
                        CurrencyOptions.Clear(); 
                        _ = RefreshCurrencyOptionsAsync(); 
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
                        CurrencyOptions.Clear(); 
                        _ = RefreshCurrencyOptionsAsync(); 
                    }
                    NotifyEnableFlags();
                }
            }
        }

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => Voucher?.VoucherID == 0 ? "سند صرف جديد" : $"تعديل سند صرف - {Voucher?.DocumentNumber}";

        // ===== ctor =====
        public PaymentVoucherEditViewModel(PaymentVoucherService service)
        {
            _service = service;

            // أوامر الإدخال
            SaveAsyncCommand = new AsyncRelayCommand(SaveAsyncImpl, () => IsEditing);
            CancelCommand = new AsyncRelayCommand(CancelImpl, () => IsEditing);
            CurrencyChangedAsyncCommand = new AsyncRelayCommand(CurrencyChangedAsyncImpl);
            AddRowCommand = new RelayCommand(AddRowImpl, () => IsEditing);
            RemoveRowCommand = new RelayCommand<PaymentVoucherDetail?>(RemoveRowImpl, _ => IsEditing);
        }

        private void RaiseCanExecutes()
        {
            (AddRowCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (RemoveRowCommand as IRelayCommand)?.NotifyCanExecuteChanged();

            SaveAsyncCommand?.NotifyCanExecuteChanged();
            CancelCommand?.NotifyCanExecuteChanged();
        }

        // ===== تحميل القوائم الأساسية =====
        public async Task LoadOptionsAsync()
        {
            BranchOptions.Clear();
            CashBoxOptions.Clear();
            BankOptions.Clear();
            CurrencyOptions.Clear();
            DocumentTypeOptions.Clear();
            CostCenterOptions.Clear();
            AccountOptions.Clear();

            foreach (var b in await _service.GetBranchesAsync()) BranchOptions.Add(b);
            foreach (var c in await _service.GetCostCentersAsync()) CostCenterOptions.Add(c);
            foreach (var a in await _service.GetAccountsAsync()) AccountOptions.Add(a);

            var pv = await _service.GetPVTypeAsync();
            if (pv != null) DocumentTypeOptions.Add(pv);
        }

        // ===== جديد/تعديل/حفظ/إلغاء =====
        public void InitializeNew()
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

        public async void InitializeEdit(int voucherId)
        {
            var v = await _service.GetByIdAsync(voucherId);
            if (v == null) return;

            _isOpeningVoucher = true;

            var tempCurrencyId = v.CurrencyID;
            var tempCashId = v.CashBoxID;
            var tempBankId = v.BankID;
            
            VoucherType = (v.VoucherType == "Cheque") ? VType.Cheque : VType.Cash;
            Voucher = v;

            SelectedBranchId = v.BranchID;
            await ReloadSourceListsForBranchAsync(SelectedBranchId);

            _isOpeningVoucher = false;
            SelectedCashBoxId = tempCashId;
            SelectedBankId = tempBankId;

            await RefreshCurrencyOptionsAsync(preserveCurrentCurrency: true);

            if (Voucher.CurrencyID != tempCurrencyId)
            {
                Voucher.CurrencyID = tempCurrencyId;
            }

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
            await _fetchLock.WaitAsync();
            try
            {
                var current = Voucher?.CurrencyID ?? 0;

                CurrencyOptions.Clear();
                if (Voucher == null) 
                {
                    return;
                }

                if (IsCash && (SelectedCashBoxId ?? 0) > 0)
                {
                    var currencies = await _service.GetCurrenciesForCashBoxAsync(SelectedCashBoxId!.Value);
                    foreach (var c in currencies)
                        CurrencyOptions.Add(c);
                }
                else if (IsCheque && (SelectedBankId ?? 0) > 0)
                {
                    var currencies = await _service.GetCurrenciesForBankAsync(SelectedBankId!.Value);
                    foreach (var c in currencies)
                        CurrencyOptions.Add(c);
                }

                // منطق الحفاظ على العملة أو تصفيرها
                if (preserveCurrentCurrency && current > 0)
                {
                    if (CurrencyOptions.Any(x => x.CurrencyId == current))
                    {
                        Voucher.CurrencyID = current;
                    }
                }
                else if (!preserveCurrentCurrency)
                {
                    if (Voucher.CurrencyID != 0 && !CurrencyOptions.Any(x => x.CurrencyId == Voucher.CurrencyID))
                    {
                        Voucher.CurrencyID = 0;
                        Voucher.ExchangeRate = 1m;
                    }
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
    }
}

