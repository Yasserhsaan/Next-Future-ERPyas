using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.StoreIssues.Models;
using Next_Future_ERP.Features.StoreIssues.Services;
using Next_Future_ERP.Features.StoreIssues.Views;
using Next_Future_ERP.Models;
using System.Collections.ObjectModel;

namespace Next_Future_ERP.Features.StoreIssues.ViewModels
{
    public partial class IssueDestinationsViewModel : ObservableObject
    {
        private readonly IIssueDestinationsService _service;
        private readonly AccountsService _accountsService;
        private readonly ICostCentersService _costCentersService;

        [ObservableProperty] private ObservableCollection<IssueDestination> destinations = new();
        [ObservableProperty] private IssueDestination? selectedDestination;
        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private bool isLoading;

        // Commands
        public IAsyncRelayCommand LoadAsyncCommand { get; }

        // Data for dropdowns
        public ObservableCollection<Next_Future_ERP.Models.Account> Accounts { get; } = new();
        public ObservableCollection<CostCenter> CostCenters { get; } = new();

        public IssueDestinationsViewModel(IIssueDestinationsService service, AccountsService accountsService, ICostCentersService costCentersService)
        {
            _service = service;
            _accountsService = accountsService;
            _costCentersService = costCentersService;
            
            // Initialize commands
            LoadAsyncCommand = new AsyncRelayCommand(LoadAsync);
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("IssueDestinationsViewModel.LoadAsync: Starting load operation");

                // Load destinations
                System.Diagnostics.Debug.WriteLine("IssueDestinationsViewModel.LoadAsync: Calling _service.GetAllAsync()");
                var destinations = await _service.GetAllAsync();
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.LoadAsync: Got {destinations?.Count() ?? 0} destinations from service");
                
                Destinations.Clear();
                if (destinations != null)
                {
                    foreach (var destination in destinations)
                    {
                        Destinations.Add(destination);
                    }
                }
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.LoadAsync: Added {Destinations.Count} destinations to collection");

                // Load accounts
                var accounts = await _accountsService.GetAllAsync();
                Accounts.Clear();
                foreach (var account in accounts)
                {
                    Accounts.Add(account);
                }

                // Load cost centers
                var costCenters = await _costCentersService.GetAllAsync();
                CostCenters.Clear();
                foreach (var costCenter in costCenters)
                {
                    CostCenters.Add(costCenter);
                }

                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.LoadAsync: Loaded {Destinations.Count} destinations");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.LoadAsync: Error: {ex.Message}");
                // Handle error - could show message to user
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.SearchAsync: Searching for '{SearchText}'");

                var destinations = await _service.GetAllAsync(SearchText);
                Destinations.Clear();
                foreach (var destination in destinations)
                {
                    Destinations.Add(destination);
                }

                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.SearchAsync: Found {Destinations.Count} destinations");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.SearchAsync: Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadSampleDataAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("IssueDestinationsViewModel.LoadSampleDataAsync: Loading sample data");

                // إنشاء بيانات افتراضية لجهات الصرف
                var sampleDestinations = new List<IssueDestination>
                {
                    // جهات الصرف الأساسية للمصروفات
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "EXP001",
                        DestinationName = "مصروفات إدارية عامة",
                        DestinationType = 'E',
                        AccountID = 101,
                        CostCenterID = 1,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "مصروفات الإدارة العامة والموظفين",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "EXP002",
                        DestinationName = "مصروفات تسويقية",
                        DestinationType = 'E',
                        AccountID = 102,
                        CostCenterID = 2,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "مصروفات التسويق والإعلان والمبيعات",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "EXP003",
                        DestinationName = "مصروفات مالية",
                        DestinationType = 'E',
                        AccountID = 103,
                        CostCenterID = 3,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "مصروفات الفوائد والعمولات المصرفية",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "EXP004",
                        DestinationName = "مصروفات صيانة",
                        DestinationType = 'E',
                        AccountID = 104,
                        CostCenterID = 4,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "مصروفات صيانة الآلات والمعدات",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "EXP005",
                        DestinationName = "مصروفات كهرباء وماء",
                        DestinationType = 'E',
                        AccountID = 105,
                        CostCenterID = 5,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "مصروفات المرافق العامة",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },

                    // جهات الصرف للإنتاج والتشغيل
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "PROD001",
                        DestinationName = "إنتاج رئيسي",
                        DestinationType = 'P',
                        AccountID = 201,
                        CostCenterID = 9,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "إنتاج المنتجات الرئيسية",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "PROD002",
                        DestinationName = "إنتاج فرعي",
                        DestinationType = 'P',
                        AccountID = 202,
                        CostCenterID = 10,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "إنتاج المنتجات الفرعية",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },

                    // جهات الصرف لتكلفة المبيعات
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "COGS001",
                        DestinationName = "تكلفة مبيعات بضائع",
                        DestinationType = 'C',
                        AccountID = 301,
                        CostCenterID = 13,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "تكلفة البضائع المباعة",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },

                    // جهات الصرف للهالك
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "SHRINK001",
                        DestinationName = "هالك طبيعي",
                        DestinationType = 'S',
                        AccountID = 401,
                        CostCenterID = 16,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "هالك طبيعي للمواد الخام",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },

                    // جهات الصرف للتسويات
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "ADJ001",
                        DestinationName = "تسوية مخزون",
                        DestinationType = 'A',
                        AccountID = 501,
                        CostCenterID = 19,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "تسوية أرصدة المخزون",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },

                    // جهات صرف أخرى
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "OTHER001",
                        DestinationName = "صرف للعملاء",
                        DestinationType = 'O',
                        AccountID = 601,
                        CostCenterID = 22,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "صرف عينات للعملاء",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },

                    // جهات صرف خاصة بالقطاع السعودي
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "KSA001",
                        DestinationName = "صرف للمشاريع الحكومية",
                        DestinationType = 'O',
                        AccountID = 701,
                        CostCenterID = 26,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "صرف مواد للمشاريع الحكومية",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    },
                    new IssueDestination
                    {
                        CompanyID = 1,
                        BranchID = 1,
                        DestinationCode = "KSA002",
                        DestinationName = "صرف للرؤية 2030",
                        DestinationType = 'O',
                        AccountID = 702,
                        CostCenterID = 27,
                        UsesCostCenter = true,
                        AllowAccountOverride = true,
                        AllowLineOverride = true,
                        Description = "صرف مواد لمشاريع الرؤية 2030",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1
                    }
                };

                // إضافة البيانات الافتراضية
                foreach (var destination in sampleDestinations)
                {
                    await _service.AddAsync(destination);
                }

                // إعادة تحميل القائمة
                await LoadAsync();

                System.Windows.MessageBox.Show(
                    "تم تحميل البيانات الافتراضية لجهات الصرف بنجاح!\n\n" +
                    "تم إضافة 12 جهة صرف مختلفة تشمل:\n" +
                    "• مصروفات إدارية وتسويقية ومالية\n" +
                    "• إنتاج رئيسي وفرعي\n" +
                    "• تكلفة مبيعات\n" +
                    "• هالك طبيعي\n" +
                    "• تسوية مخزون\n" +
                    "• صرف للعملاء والمشاريع الحكومية\n" +
                    "• مشاريع الرؤية 2030",
                    "تم تحميل البيانات الافتراضية",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.LoadSampleDataAsync: Error: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"خطأ أثناء تحميل البيانات الافتراضية:\n{ex.Message}",
                    "خطأ",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void AddNew()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("IssueDestinationsViewModel.AddNew: Opening add window");

                var newDestination = new IssueDestination
                {
                    CompanyID = 1, // TODO: Get from current context
                    BranchID = 1,  // TODO: Get from current context
                    DestinationCode = string.Empty,
                    DestinationName = string.Empty,
                    DestinationType = 'E', // Default to Expense
                    IsActive = true,
                    UsesCostCenter = false,
                    AllowAccountOverride = false,
                    AllowLineOverride = false
                };

                var editWindow = new IssueDestinationEditWindow(newDestination, _service, _accountsService, _costCentersService);
                editWindow.Owner = System.Windows.Application.Current.MainWindow;
                
                if (editWindow.ShowDialog() == true)
                {
                    _ = LoadAsync(); // Refresh the list
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.AddNew: Error: {ex.Message}");
            }
        }

        [RelayCommand]
        public void EditSelected()
        {
            if (SelectedDestination == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.EditSelected: Editing destination ID: {SelectedDestination.DestinationID}");

                var editWindow = new IssueDestinationEditWindow(SelectedDestination, _service, _accountsService, _costCentersService);
                editWindow.Owner = System.Windows.Application.Current.MainWindow;
                
                if (editWindow.ShowDialog() == true)
                {
                    _ = LoadAsync(); // Refresh the list
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.EditSelected: Error: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task DeleteSelectedAsync()
        {
            try
            {
                if (SelectedDestination == null)
                {
                    System.Windows.MessageBox.Show(
                        "يرجى اختيار جهة صرف للحذف.",
                        "لا يوجد اختيار",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.DeleteSelectedAsync: Deleting destination ID: {SelectedDestination.DestinationID}");

                var result = System.Windows.MessageBox.Show(
                    $"هل أنت متأكد من حذف جهة الصرف '{SelectedDestination.DestinationName}'؟",
                    "تأكيد الحذف",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    await _service.DeleteAsync(SelectedDestination.DestinationID);
                    await LoadAsync(); // Refresh the list
                    
                    System.Windows.MessageBox.Show(
                        "تم حذف جهة الصرف بنجاح.",
                        "تم الحذف",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsViewModel.DeleteSelectedAsync: Error: {ex.Message}");
                
                System.Windows.MessageBox.Show(
                    $"خطأ في حذف جهة الصرف:\n\n{ex.Message}",
                    "خطأ في الحذف",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Destination type options for ComboBox
        public List<DestinationTypeOption> DestinationTypeOptions { get; } = new()
        {
            new DestinationTypeOption { Value = 'E', Text = "مصروف" },
            new DestinationTypeOption { Value = 'P', Text = "تشغيل" },
            new DestinationTypeOption { Value = 'C', Text = "تكلفة مبيعات" },
            new DestinationTypeOption { Value = 'S', Text = "هالك" },
            new DestinationTypeOption { Value = 'A', Text = "تسوية" },
            new DestinationTypeOption { Value = 'O', Text = "أخرى" }
        };
    }
}
