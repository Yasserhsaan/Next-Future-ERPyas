using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class OpeningBalanceViewModel : ObservableObject
    {
        private readonly IOpeningBalanceService _openingBalanceService;
        private readonly IReferenceDataService _referenceDataService;
        private readonly OpeningBalanceValidationService _validationService;

        #region Commands
        public IAsyncRelayCommand LoadDataCommand { get; }
        public IAsyncRelayCommand NewBatchCommand { get; }
        public IAsyncRelayCommand SaveDraftCommand { get; }
        public IAsyncRelayCommand PostBatchCommand { get; }
        public IAsyncRelayCommand DeleteBatchCommand { get; }
        public IRelayCommand AddLineCommand { get; }
        public IRelayCommand<OpeningBalanceLine?> RemoveLineCommand { get; }
        public IAsyncRelayCommand OpenSearchCommand { get; }
        public IAsyncRelayCommand<OpeningBalanceBatch?> LoadBatchCommand { get; }
        public IAsyncRelayCommand ImportFromExcelCommand { get; }
        public IRelayCommand DownloadTemplateCommand { get; }
        #endregion

        #region Properties
        
        // Current Batch
        private OpeningBalanceBatch? _currentBatch;
        public OpeningBalanceBatch? CurrentBatch
        {
            get => _currentBatch;
            set
            {
                if (SetProperty(ref _currentBatch, value))
                {
                    OnPropertyChanged(nameof(IsEditable));
                    OnPropertyChanged(nameof(IsPosted));
                    OnPropertyChanged(nameof(StatusText));
                    UpdateCanExecuteCommands();
                }
            }
        }

        // Lines Collection
        public ObservableCollection<OpeningBalanceLine> Lines { get; } = new();
        
        // Selected Line
        private OpeningBalanceLine? _selectedLine;
        public OpeningBalanceLine? SelectedLine
        {
            get => _selectedLine;
            set => SetProperty(ref _selectedLine, value);
        }

        // Lookup Collections
        public ObservableCollection<CompanyInfoModel> Companies { get; } = new();
        public ObservableCollection<BranchModel> Branches { get; } = new();
        public ObservableCollection<Fund> Funds { get; } = new();
        public ObservableCollection<Bank> Banks { get; } = new();
        public ObservableCollection<Account> Accounts { get; } = new();
        public ObservableCollection<CostCenter> CostCenters { get; } = new();
        public ObservableCollection<NextCurrency> Currencies { get; } = new();

        // Current Selections
        private int _selectedCompanyId = 0; // Start with 0 until we load companies
        public int SelectedCompanyId
        {
            get => _selectedCompanyId;
            set
            {
                if (SetProperty(ref _selectedCompanyId, value))
                {
                    // Update current batch if exists
                    if (CurrentBatch != null) CurrentBatch.CompanyId = value;
                    
                    // Clear dependent selections
                    SelectedBranchId = 0; // Use property to trigger setter
                    Branches.Clear();
                    Funds.Clear();
                    Banks.Clear();
                    Accounts.Clear();
                    CostCenters.Clear();
                    
                    // Load dependent data
                    _branchLoadTask = LoadBranchesForCompanyAsync(value);
                    
                    // Load currencies and update company currency after loading
                    _ = Task.Run(async () =>
                    {
                        await LoadCurrenciesAsync();
                    UpdateCompanyCurrency();
                    });
                }
            }
        }

        private int _selectedBranchId = 0; // Start with 0 until we load branches
        public int SelectedBranchId
        {
            get => _selectedBranchId;
            set
            {
                if (SetProperty(ref _selectedBranchId, value))
                {
                    // Update current batch if exists
                    if (CurrentBatch != null) CurrentBatch.BranchId = value;
                    
                    // Clear dependent selections
                    Funds.Clear();
                    Banks.Clear();
                    Accounts.Clear();
                    CostCenters.Clear();
                    
                    // Load dependent data for branch
                    _ = LoadFundsForBranchAsync();
                    _ = LoadBanksForBranchAsync();
                    _ = LoadAccountsAsync();
                    _ = LoadCostCentersAsync();
                }
            }
        }

        private short _fiscalYear = (short)DateTime.Now.Year;
        public short FiscalYear
        {
            get => _fiscalYear;
            set 
            {
                if (SetProperty(ref _fiscalYear, value))
                {
                    // تحديث رقم الدفعة والسنة المالية في الدفعة الحالية
                    if (CurrentBatch != null)
                    {
                        CurrentBatch.FiscalYear = value;
                        UpdateBatchNumber();
                    }
                }
            }
        }

        // Company Currency
        private NextCurrency? _companyCurrency;
        public NextCurrency? CompanyCurrency
        {
            get => _companyCurrency;
            set
            {
                if (SetProperty(ref _companyCurrency, value))
                {
                    OnPropertyChanged(nameof(CompanyCurrencyName));
                    UpdateAllLinesCurrency();
                }
            }
        }

        public string CompanyCurrencyName => CompanyCurrency?.CurrencyNameAr ?? "غير محدد";

        // Totals
        public decimal CompanyDebitTotal => Lines.Sum(l => l.CompanyDebit);
        public decimal CompanyCreditTotal => Lines.Sum(l => l.CompanyCredit);
        public decimal CompanyNetBalance => CompanyDebitTotal - CompanyCreditTotal;
        public bool IsBalanced => Math.Abs(CompanyNetBalance) < 0.01m;

        // UI State
        public bool IsEditable => CurrentBatch?.IsDraft ?? true;
        public bool IsPosted => CurrentBatch?.IsPosted ?? false;
        public string StatusText => CurrentBatch?.StatusText ?? "جديد";

        // Search Panel
        private bool _isSearchPanelVisible;
        public bool IsSearchPanelVisible
        {
            get => _isSearchPanelVisible;
            set => SetProperty(ref _isSearchPanelVisible, value);
        }

        // Search Results
        public ObservableCollection<OpeningBalanceBatch> SearchResults { get; } = new();

        // Search Filters
        private string? _searchDocNo;
        public string? SearchDocNo
        {
            get => _searchDocNo;
            set => SetProperty(ref _searchDocNo, value);
        }

        private DateTime? _searchDateFrom;
        public DateTime? SearchDateFrom
        {
            get => _searchDateFrom;
            set => SetProperty(ref _searchDateFrom, value);
        }

        private DateTime? _searchDateTo;
        public DateTime? SearchDateTo
        {
            get => _searchDateTo;
            set => SetProperty(ref _searchDateTo, value);
        }

        private byte? _searchStatus;
        public byte? SearchStatus
        {
            get => _searchStatus;
            set => SetProperty(ref _searchStatus, value);
        }

        private short? _searchFiscalYear;
        public short? SearchFiscalYear
        {
            get => _searchFiscalYear;
            set => SetProperty(ref _searchFiscalYear, value);
        }

        #endregion

        #region Private Fields for Async Loading
        // Loading synchronization exactly like FundsViewModel and BanksViewModel
        private readonly SemaphoreSlim _branchLock = new(1, 1);
        private Task _branchLoadTask = Task.CompletedTask;
        private int _lastCompanyIdLoaded = -1;
        private bool _isLoading = false;
        #endregion

        #region Constructor
        public OpeningBalanceViewModel(IOpeningBalanceService openingBalanceService, IReferenceDataService referenceDataService)
        {
            _openingBalanceService = openingBalanceService;
            _referenceDataService = referenceDataService;
            _validationService = new OpeningBalanceValidationService(referenceDataService);

            // Initialize Commands
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            NewBatchCommand = new AsyncRelayCommand(NewBatchAsync);
            SaveDraftCommand = new AsyncRelayCommand(SaveDraftAsync, CanSaveDraft);
            PostBatchCommand = new AsyncRelayCommand(PostBatchAsync, CanPostBatch);
            DeleteBatchCommand = new AsyncRelayCommand(DeleteBatchAsync, CanDeleteBatch);
            AddLineCommand = new RelayCommand(AddLine, CanAddLine);
            RemoveLineCommand = new RelayCommand<OpeningBalanceLine?>(RemoveLine);
            OpenSearchCommand = new AsyncRelayCommand(OpenSearchAsync);
            LoadBatchCommand = new AsyncRelayCommand<OpeningBalanceBatch?>(LoadBatchAsync);
            ImportFromExcelCommand = new AsyncRelayCommand(ImportFromExcelAsync);
            DownloadTemplateCommand = new RelayCommand(DownloadTemplate);

            // Subscribe to Lines collection changes
            Lines.CollectionChanged += (s, e) => UpdateTotals();

            // Initialize with empty batch
            InitializeNewBatch();
        }
        #endregion

        #region Data Loading Methods
        private async Task LoadDataAsync()
        {
            try
            {
                // Step 1: Load companies first
                await LoadCompaniesAsync();
                
                // Step 2: Auto-select first company if none selected (without triggering setter)
                if (SelectedCompanyId <= 0 && Companies.Any())
                {
                    // Directly set the backing field to avoid triggering setter
                    _selectedCompanyId = Companies.First().CompId;
                    if (CurrentBatch != null) CurrentBatch.CompanyId = _selectedCompanyId;
                    OnPropertyChanged(nameof(SelectedCompanyId));
                }
                
                // Step 3: Load branches for selected company
                if (SelectedCompanyId > 0)
                {
                    await LoadBranchesForCompanyAsync(SelectedCompanyId);
                    
                    // Step 4: Auto-select first branch if none selected (without triggering setter)
                    if (SelectedBranchId <= 0 && Branches.Any())
                    {
                        // Directly set the backing field to avoid triggering setter
                        _selectedBranchId = Branches.First().BranchId;
                        if (CurrentBatch != null) CurrentBatch.BranchId = _selectedBranchId;
                        OnPropertyChanged(nameof(SelectedBranchId));
                    }
                }
                
                // Step 5: Load all dependent data sequentially
                if (SelectedBranchId > 0)
                {
                    await LoadFundsForBranchAsync();
                    await LoadBanksForBranchAsync();
                await LoadAccountsAsync();
                await LoadCostCentersAsync();
                }
                
                // Step 6: Load currencies and set company currency
                await LoadCurrenciesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCompaniesAsync()
        {
            try
            {
                var companies = await _referenceDataService.GetCompaniesAsync();
                Companies.Clear();
                foreach (var company in companies)
                {
                    Companies.Add(company);
                }
                
                // Set default company if none selected
                if (SelectedCompanyId <= 0 && Companies.Any())
                {
                    SelectedCompanyId = Companies.First().CompId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الشركات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadBranchesForCompanyAsync(int companyId)
        {
            await _branchLock.WaitAsync();
            try
            {
                if (companyId <= 0)
                {
                    Branches.Clear();
                    _lastCompanyIdLoaded = 0;
                    return;
                }

                // If same company already loaded and branches exist, don't reload
                if (_lastCompanyIdLoaded == companyId && Branches.Count > 0)
                    return;

                Branches.Clear();
                var branches = await _referenceDataService.GetBranchesForCompanyAsync(companyId);
                foreach (var branch in branches)
                {
                    Branches.Add(branch);
                }
                _lastCompanyIdLoaded = companyId;

                // If selected branch is not in current company's branches, reset
                if (SelectedBranchId != 0 && !Branches.Any(b => b.BranchId == SelectedBranchId))
                {
                    _selectedBranchId = 0;
                    if (CurrentBatch != null) CurrentBatch.BranchId = 0;
                }
                
                // Auto-select first branch if none selected
                if (Branches.Count > 0 && SelectedBranchId == 0)
                {
                    _selectedBranchId = Branches[0].BranchId;
                    if (CurrentBatch != null) CurrentBatch.BranchId = _selectedBranchId;
                    OnPropertyChanged(nameof(SelectedBranchId));
                }
            }
            finally
            {
                _branchLock.Release();
            }
        }

        private async Task LoadFundsForBranchAsync()
        {
            try
            {
                if (SelectedCompanyId <= 0 || SelectedBranchId <= 0)
                {
                    Funds.Clear();
                    return;
                }

                var funds = await _referenceDataService.GetFundsForBranchAsync(SelectedCompanyId, SelectedBranchId);
                Funds.Clear();
                foreach (var fund in funds)
                {
                    Funds.Add(fund);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الصناديق: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadBanksForBranchAsync()
        {
            try
            {
                if (SelectedCompanyId <= 0 || SelectedBranchId <= 0)
                {
                    Banks.Clear();
                    return;
                }

                var banks = await _referenceDataService.GetBanksForBranchAsync(SelectedCompanyId, SelectedBranchId);
                Banks.Clear();
                foreach (var bank in banks)
                {
                    Banks.Add(bank);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البنوك: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAccountsAsync()
        {
            try
            {
                // تحميل الحسابات التي تقبل الحركة فقط (AccountType = 2)
                var allAccounts = await _referenceDataService.GetLeafAccountsAsync(SelectedCompanyId, SelectedBranchId);
                var movableAccounts = allAccounts.Where(a => a.AccountType == 2).ToList();
                
                Accounts.Clear();
                foreach (var account in movableAccounts)
                {
                    Accounts.Add(account);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الحسابات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCostCentersAsync()
        {
            try
            {
                var costCenters = await _referenceDataService.GetCostCentersAsync(SelectedCompanyId, SelectedBranchId);
                CostCenters.Clear();
                foreach (var cc in costCenters)
                {
                    CostCenters.Add(cc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل مراكز التكلفة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCurrenciesAsync()
        {
            try
            {
                // Only load currencies if we have a valid company selected
                if (SelectedCompanyId <= 0)
                {
                Currencies.Clear();
                    CompanyCurrency = null;
                    return;
                }

                var allCurrencies = await _referenceDataService.GetCurrenciesAsync(SelectedCompanyId);
                
                Currencies.Clear();
                foreach (var currency in allCurrencies)
                {
                    Currencies.Add(currency);
                }
                
                // تحديث عملة الشركة الافتراضية
                UpdateCompanyCurrency();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل العملات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCompanyCurrency()
        {
            try
            {
                // Debug: Log currency count
                System.Diagnostics.Debug.WriteLine($"UpdateCompanyCurrency: Currencies.Count = {Currencies.Count}");
                
                if (!Currencies.Any())
                {
                    CompanyCurrency = null;
                    return;
                }

                // البحث عن عملة الشركة الافتراضية (IsCompanyCurrency = true)
                CompanyCurrency = Currencies.FirstOrDefault(c => c.IsCompanyCurrency == true);
                
                // Debug: Log found currency
                System.Diagnostics.Debug.WriteLine($"Company default currency: {CompanyCurrency?.CurrencyNameAr ?? "null"}");
                
                // إذا لم توجد عملة افتراضية، استخدم أول عملة متاحة
                if (CompanyCurrency == null && Currencies.Any())
                {
                    CompanyCurrency = Currencies.First();
                    System.Diagnostics.Debug.WriteLine($"Using first currency: {CompanyCurrency?.CurrencyNameAr ?? "null"}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateCompanyCurrency error: {ex.Message}");
                MessageBox.Show($"خطأ في تحديد عملة الشركة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Batch Operations
        private async Task NewBatchAsync()
        {
            try
            {
                if (HasUnsavedChanges())
                {
                    var result = MessageBox.Show("هناك تغييرات غير محفوظة. هل تريد المتابعة؟", 
                        "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes)
                        return;
                }

                InitializeNewBatch();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء دفعة جديدة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeNewBatch()
        {
            CurrentBatch = new OpeningBalanceBatch
            {
                CompanyId = SelectedCompanyId,
                BranchId = SelectedBranchId,
                FiscalYear = FiscalYear,
                DocDate = DateTime.Today,
                CreatedBy = 1 // Should get from current user session
            };

            // تحديث رقم الدفعة ليشمل السنة المالية
            UpdateBatchNumber();

            Lines.Clear();
            UpdateTotals();
        }

        private void UpdateBatchNumber()
        {
            if (CurrentBatch != null)
            {
                // تنسيق رقم الدفعة: FY{سنة مالية}-OB-{رقم تسلسلي}
                CurrentBatch.BatchCode = $"FY{FiscalYear}-OB-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";
            }
        }

        private async Task SaveDraftAsync()
        {
            try
            {
                if (!await ValidateBeforeSaveAsync())
                    return;

                if (CurrentBatch == null)
                    return;

                CurrentBatch.CompanyId = SelectedCompanyId;
                CurrentBatch.BranchId = SelectedBranchId;
                CurrentBatch.FiscalYear = FiscalYear;

                var batchId = await _openingBalanceService.CreateOrUpdateDraftAsync(CurrentBatch, Lines.ToList());
                
                if (batchId > 0)
                {
                    CurrentBatch.BatchId = batchId;
                    MessageBox.Show("تم حفظ المسودة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateCanExecuteCommands();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ المسودة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PostBatchAsync()
        {
            try
            {
                if (CurrentBatch == null || CurrentBatch.BatchId <= 0)
                {
                    MessageBox.Show("يجب حفظ المسودة أولاً", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!IsBalanced)
                {
                    MessageBox.Show($"الدفعة غير متوازنة. الفرق: {CompanyNetBalance:N4}", 
                        "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show("هل أنت متأكد من ترحيل هذه الدفعة؟ لا يمكن التراجع عن هذا الإجراء.", 
                    "تأكيد الترحيل", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _openingBalanceService.PostBatchAsync(CurrentBatch.BatchId, 1); // Current user ID
                    
                    if (success)
                    {
                        CurrentBatch.Status = 1; // Posted
                        CurrentBatch.PostedAt = DateTime.UtcNow;
                        MessageBox.Show("تم ترحيل الدفعة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                        UpdateCanExecuteCommands();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في ترحيل الدفعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteBatchAsync()
        {
            try
            {
                if (CurrentBatch == null || CurrentBatch.BatchId <= 0)
                    return;

                if (CurrentBatch.IsPosted)
                {
                    MessageBox.Show("لا يمكن حذف دفعة مُرحلة", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show("هل أنت متأكد من حذف هذه الدفعة؟", 
                    "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _openingBalanceService.DeleteDraftAsync(CurrentBatch.BatchId);
                    
                    if (success)
                    {
                        MessageBox.Show("تم حذف الدفعة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                        InitializeNewBatch();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حذف الدفعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadBatchAsync(OpeningBalanceBatch? batch)
        {
            try
            {
                if (batch == null)
                    return;

                CurrentBatch = batch;
                
                // Update selections to match batch - this will trigger loading dependent data
                SelectedCompanyId = batch.CompanyId; // This will trigger loading branches
                await _branchLoadTask; // Wait for branch loading to complete
                SelectedBranchId = batch.BranchId; // This will trigger loading funds, banks, accounts, etc.
                FiscalYear = batch.FiscalYear;

                // Load full data to ensure everything is available
                await LoadDataAsync();

                var lines = await _openingBalanceService.GetBatchLinesAsync(batch.BatchId);
                Lines.Clear();
                foreach (var line in lines)
                {
                    // Subscribe to property changes for calculations
                    line.PropertyChanged += OnLinePropertyChanged;
                    Lines.Add(line);
                }

                UpdateTotals();
                IsSearchPanelVisible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الدفعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Line Operations
        private void AddLine()
        {
            var newLine = new OpeningBalanceLine
            {
                BatchId = CurrentBatch?.BatchId ?? 0,
                CompanyCurrencyId = CompanyCurrency?.CurrencyId ?? 0,
                TransactionCurrencyId = CompanyCurrency?.CurrencyId ?? 0,
                ExchangeRate = 1
            };

            // تحديث اسم عملة المعاملة
            UpdateTransactionCurrencyName(newLine);

            // Subscribe to property changes for automatic calculations
            newLine.PropertyChanged += OnLinePropertyChanged;
            
            Lines.Add(newLine);
            SelectedLine = newLine;
            UpdateTotals();
        }

        private void RemoveLine(OpeningBalanceLine? line)
        {
            if (line != null && Lines.Contains(line))
            {
                line.PropertyChanged -= OnLinePropertyChanged;
                Lines.Remove(line);
                UpdateTotals();
            }
        }

        private void UpdateTransactionCurrencyName(OpeningBalanceLine line)
        {
            if (line.TransactionCurrencyId > 0)
            {
                var currency = Currencies.FirstOrDefault(c => c.CurrencyId == line.TransactionCurrencyId);
                line.TransactionCurrencyName = currency?.CurrencyNameAr ?? "غير محدد";
            }
            else
            {
                line.TransactionCurrencyName = "غير محدد";
            }
        }

        private void OnLinePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OpeningBalanceLine.CompanyDebit) || 
                e.PropertyName == nameof(OpeningBalanceLine.CompanyCredit))
            {
                UpdateTotals();
            }

            if (sender is OpeningBalanceLine line)
            {
                // Update account-related properties when account changes
                if (e.PropertyName == nameof(OpeningBalanceLine.AccountId))
                {
                    UpdateLineAccountInfo(line);
                }

                // Update currency info when transaction currency changes
                if (e.PropertyName == nameof(OpeningBalanceLine.TransactionCurrencyId))
                {
                    UpdateTransactionCurrencyName(line);
                    UpdateLineCurrencyInfo(line);
                }
            }
        }

        private async void UpdateLineAccountInfo(OpeningBalanceLine line)
        {
            try
            {
                if (line.AccountId > 0)
                {
                    var account = await _referenceDataService.GetAccountDetailsAsync(line.AccountId);
                    if (account != null)
                    {
                        line.AccountCode = account.AccountCode;
                        line.AccountNameAr = account.AccountNameAr;
                        line.UsesCostCenter = account.UsesCostCenter ?? false;
                        
                        // Clear cost center if not required
                        if (!line.UsesCostCenter)
                        {
                            line.CostCenterId = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل بيانات الحساب: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateLineCurrencyInfo(OpeningBalanceLine line)
        {
            var currency = Currencies.FirstOrDefault(c => c.CurrencyId == line.TransactionCurrencyId);
            if (currency != null)
            {
                line.TransactionCurrencyName = currency.CurrencyNameAr;
                
                // Update exchange rate if same as company currency
                if (line.TransactionCurrencyId == CompanyCurrency?.CurrencyId)
                {
                    line.ExchangeRate = 1;
                }
            }
        }

        private void UpdateAllLinesCurrency()
        {
            if (CompanyCurrency != null)
            {
                foreach (var line in Lines)
                {
                    line.CompanyCurrencyId = CompanyCurrency.CurrencyId;
                    line.CompanyCurrencyName = CompanyCurrency.CurrencyNameAr;
                }
            }
        }
        #endregion

        #region Search Operations
        private async Task OpenSearchAsync()
        {
            try
            {
                IsSearchPanelVisible = !IsSearchPanelVisible;
                
                if (IsSearchPanelVisible)
                {
                    await SearchBatchesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح البحث: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task SearchBatchesAsync()
        {
            try
            {
                var filter = new OpeningBalanceSearchFilter
                {
                    CompanyId = SelectedCompanyId,
                    BranchId = SelectedBranchId,
                    FiscalYear = SearchFiscalYear,
                    DocNo = SearchDocNo,
                    DateFrom = SearchDateFrom,
                    DateTo = SearchDateTo,
                    Status = SearchStatus
                };

                var results = await _openingBalanceService.SearchBatchesAsync(filter);
                
                SearchResults.Clear();
                foreach (var batch in results)
                {
                    SearchResults.Add(batch);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Import/Export Operations
        private async Task ImportFromExcelAsync()
        {
            try
            {
                // Create OpenFileDialog
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    Title = "اختر ملف Excel للاستيراد"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var filePath = openFileDialog.FileName;
                    
                    // Import data from Excel
                    var importService = new OpeningBalanceImportService(_referenceDataService);
                    var importResult = await importService.ImportFromExcelAsync(filePath, SelectedCompanyId, SelectedBranchId);
                    
                    if (importResult.IsSuccess)
                    {
                        // Clear existing lines and add imported ones
                        Lines.Clear();
                        foreach (var line in importResult.Lines)
                        {
                            line.PropertyChanged += OnLinePropertyChanged;
                            Lines.Add(line);
                        }
                        
                        UpdateTotals();
                        MessageBox.Show($"تم استيراد {importResult.Lines.Count} سطر بنجاح", "نجح الاستيراد", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var errorMessage = "فشل في استيراد الملف:\n" + string.Join("\n", importResult.Errors);
                        MessageBox.Show(errorMessage, "خطأ في الاستيراد", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في استيراد الملف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadTemplate()
        {
            try
            {
                // Create SaveFileDialog
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    Title = "حفظ نموذج Excel",
                    FileName = "نموذج_الأرصدة_الافتتاحية.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;
                    
                    // Create template
                    var importService = new OpeningBalanceImportService(_referenceDataService);
                    importService.CreateTemplate(filePath, Accounts.ToList(), Currencies.ToList(), CostCenters.ToList());
                    
                    MessageBox.Show("تم إنشاء النموذج بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Open the file
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء النموذج: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Validation & Helper Methods
        private async Task<bool> ValidateBeforeSaveAsync()
        {
            if (CurrentBatch == null)
                return false;

            try
            {
                var validation = await _validationService.ValidateCompleteBatchAsync(CurrentBatch, Lines.ToList());
                
                if (!validation.IsValid)
                {
                    var errorMessage = "توجد أخطاء في البيانات:\n" + string.Join("\n", validation.Errors.Take(10));
                    if (validation.Errors.Count > 10)
                        errorMessage += $"\n... و {validation.Errors.Count - 10} خطأ إضافي";
                        
                    MessageBox.Show(errorMessage, "أخطاء في البيانات", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (validation.Warnings.Any())
                {
                    var warningMessage = "توجد تحذيرات:\n" + string.Join("\n", validation.Warnings.Take(5));
                    if (validation.Warnings.Count > 5)
                        warningMessage += $"\n... و {validation.Warnings.Count - 5} تحذير إضافي";
                    warningMessage += "\n\nهل تريد المتابعة؟";
                    
                    var result = MessageBox.Show(warningMessage, "تحذيرات", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes)
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التحقق من البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool HasUnsavedChanges()
        {
            // Simple implementation - can be enhanced
            return CurrentBatch?.BatchId == 0 && Lines.Any();
        }

        private void UpdateTotals()
        {
            OnPropertyChanged(nameof(CompanyDebitTotal));
            OnPropertyChanged(nameof(CompanyCreditTotal));
            OnPropertyChanged(nameof(CompanyNetBalance));
            OnPropertyChanged(nameof(IsBalanced));
            UpdateCanExecuteCommands();
        }

        private void UpdateCanExecuteCommands()
        {
            SaveDraftCommand.NotifyCanExecuteChanged();
            PostBatchCommand.NotifyCanExecuteChanged();
            DeleteBatchCommand.NotifyCanExecuteChanged();
            AddLineCommand.NotifyCanExecuteChanged();
        }
        #endregion

        #region Command Can Execute Methods
        private bool CanSaveDraft() => IsEditable && Lines.Any();
        private bool CanPostBatch() => CurrentBatch?.IsDraft == true && IsBalanced && Lines.Any();
        private bool CanDeleteBatch() => CurrentBatch?.IsDraft == true && CurrentBatch?.BatchId > 0;
        private bool CanAddLine() => IsEditable;
        #endregion

    }
}
