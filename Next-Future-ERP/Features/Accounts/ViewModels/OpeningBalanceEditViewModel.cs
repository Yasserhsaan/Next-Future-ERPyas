using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Models;
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
    public partial class OpeningBalanceEditViewModel : ObservableObject
    {
        private readonly IOpeningBalanceService _openingBalanceService;
        private readonly IReferenceDataService _referenceDataService;
        private readonly OpeningBalanceValidationService _validationService;

        [ObservableProperty] 
        private OpeningBalanceBatch currentBatch = new();

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
        private int _selectedCompanyId = 0;
        public int SelectedCompanyId
        {
            get => _selectedCompanyId;
            set
            {
                if (SetProperty(ref _selectedCompanyId, value))
                {
                    if (CurrentBatch != null) CurrentBatch.CompanyId = value;
                    SelectedBranchId = 0;
                    Branches.Clear();
                    Funds.Clear();
                    Banks.Clear();
                    Accounts.Clear();
                    CostCenters.Clear();
                    _branchLoadTask = LoadBranchesForCompanyAsync(value);
                    _ = LoadCurrenciesAsync();
                }
            }
        }

        private int _selectedBranchId = 0;
        public int SelectedBranchId
        {
            get => _selectedBranchId;
            set
            {
                if (SetProperty(ref _selectedBranchId, value))
                {
                    if (CurrentBatch != null) CurrentBatch.BranchId = value;
                    Funds.Clear();
                    Banks.Clear();
                    Accounts.Clear();
                    CostCenters.Clear();
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
                    if (CurrentBatch != null)
                    {
                        CurrentBatch.FiscalYear = value;
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

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => CurrentBatch.BatchId == 0 ? "دفعة أرصدة افتتاحية جديدة" : "تعديل دفعة الأرصدة الافتتاحية";

        // Loading synchronization
        private readonly SemaphoreSlim _branchLock = new(1, 1);
        private Task _branchLoadTask = Task.CompletedTask;
        private int _lastCompanyIdLoaded = -1;

        public OpeningBalanceEditViewModel(IOpeningBalanceService openingBalanceService, IReferenceDataService referenceDataService)
        {
            _openingBalanceService = openingBalanceService;
            _referenceDataService = referenceDataService;
            _validationService = new OpeningBalanceValidationService(referenceDataService);

            // Subscribe to Lines collection changes
            Lines.CollectionChanged += (s, e) => UpdateTotals();
        }

        public async Task LoadOptionsAsync()
        {
            await LoadCompaniesAsync();
            if (Companies.Any() && SelectedCompanyId <= 0)
            {
                SelectedCompanyId = Companies.First().CompId;
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

                if (_lastCompanyIdLoaded == companyId && Branches.Count > 0)
                    return;

                Branches.Clear();
                var branches = await _referenceDataService.GetBranchesForCompanyAsync(companyId);
                foreach (var branch in branches)
                {
                    Branches.Add(branch);
                }
                _lastCompanyIdLoaded = companyId;

                if (SelectedBranchId != 0 && !Branches.Any(b => b.BranchId == SelectedBranchId))
                {
                    SelectedBranchId = 0;
                }
                if (Branches.Count > 0 && SelectedBranchId == 0)
                {
                    SelectedBranchId = Branches[0].BranchId;
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
                if (SelectedCompanyId <= 0 || SelectedBranchId <= 0)
                {
                    Accounts.Clear();
                    return;
                }

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
                if (SelectedCompanyId <= 0 || SelectedBranchId <= 0)
                {
                    CostCenters.Clear();
                    return;
                }

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
                if (!Currencies.Any())
                {
                    CompanyCurrency = null;
                    return;
                }

                CompanyCurrency = Currencies.FirstOrDefault(c => c.IsCompanyCurrency == true);
                
                if (CompanyCurrency == null && Currencies.Any())
                {
                    CompanyCurrency = Currencies.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحديد عملة الشركة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void InitializeNew()
        {
            CurrentBatch = new OpeningBalanceBatch
            {
                CompanyId = SelectedCompanyId,
                BranchId = SelectedBranchId,
                FiscalYear = FiscalYear,
                DocDate = DateTime.Today,
                CreatedBy = 1,
                Status = 0
            };

            Lines.Clear();
            UpdateTotals();
        }

        public async void InitializeEdit(OpeningBalanceBatch batch)
        {
            CurrentBatch = Clone(batch);
            
            SelectedCompanyId = CurrentBatch.CompanyId;
            await _branchLoadTask;
            SelectedBranchId = CurrentBatch.BranchId;
            FiscalYear = CurrentBatch.FiscalYear;

            // Load all dependent data
            await LoadFundsForBranchAsync();
            await LoadBanksForBranchAsync();
            await LoadAccountsAsync();
            await LoadCostCentersAsync();
            await LoadCurrenciesAsync();

            // Load lines
            var lines = await _openingBalanceService.GetBatchLinesAsync(batch.BatchId);
            Lines.Clear();
            foreach (var line in lines)
            {
                line.PropertyChanged += OnLinePropertyChanged;
                Lines.Add(line);
            }

            UpdateTotals();
        }

        [RelayCommand]
        public async Task SaveCommand()
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
                    CloseRequested?.Invoke(this, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ المسودة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task PostCommand()
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
                    var success = await _openingBalanceService.PostBatchAsync(CurrentBatch.BatchId, 1);
                    
                    if (success)
                    {
                        CurrentBatch.Status = 1;
                        MessageBox.Show("تم ترحيل الدفعة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                        CloseRequested?.Invoke(this, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في ترحيل الدفعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand] 
        public void Cancel() => CloseRequested?.Invoke(this, false);

        [RelayCommand]
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

        [RelayCommand]
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

        [RelayCommand]
        private void AddLine()
        {
            var newLine = new OpeningBalanceLine
            {
                BatchId = CurrentBatch?.BatchId ?? 0,
                CompanyCurrencyId = CompanyCurrency?.CurrencyId ?? 0,
                TransactionCurrencyId = CompanyCurrency?.CurrencyId ?? 0,
                ExchangeRate = 1
            };

            UpdateTransactionCurrencyName(newLine);
            newLine.PropertyChanged += OnLinePropertyChanged;
            
            Lines.Add(newLine);
            SelectedLine = newLine;
            UpdateTotals();
        }

        [RelayCommand]
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
                if (e.PropertyName == nameof(OpeningBalanceLine.AccountId))
                {
                    _ = UpdateLineAccountInfo(line);
                }

                if (e.PropertyName == nameof(OpeningBalanceLine.TransactionCurrencyId))
                {
                    UpdateTransactionCurrencyName(line);
                    UpdateLineCurrencyInfo(line);
                }
            }
        }

        private async Task UpdateLineAccountInfo(OpeningBalanceLine line)
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

        private void UpdateTotals()
        {
            OnPropertyChanged(nameof(CompanyDebitTotal));
            OnPropertyChanged(nameof(CompanyCreditTotal));
            OnPropertyChanged(nameof(CompanyNetBalance));
            OnPropertyChanged(nameof(IsBalanced));
        }

        private static OpeningBalanceBatch Clone(OpeningBalanceBatch model) => new()
        {
            BatchId = model.BatchId,
            CompanyId = model.CompanyId,
            BranchId = model.BranchId,
            FiscalYear = model.FiscalYear,
            DocNo = model.DocNo,
            DocDate = model.DocDate,
            Description = model.Description,
            Status = model.Status,
            CreatedBy = model.CreatedBy,
            CreatedAt = model.CreatedAt,
            PostedBy = model.PostedBy,
            PostedAt = model.PostedAt
        };
    }
}

