using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.Auth.Services;
using Next_Future_ERP.Features.Auth.ViewModels;
using Next_Future_ERP.Features.Auth.Views;
using Next_Future_ERP.Features.Dashboard.ViewModels;
using Next_Future_ERP.Features.Dashboard.Services;

using Next_Future_ERP.Features.Inventory.Services;
using Next_Future_ERP.Features.Inventory.ViewModels;
using Next_Future_ERP.Features.Inventory.Views;

using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Items.ViewModels;

using Next_Future_ERP.Features.Payments.Services;
using Next_Future_ERP.Features.Payments.ViewModels;
using Next_Future_ERP.Features.Permissions.Services;
using Next_Future_ERP.Features.Permissions.ViewModels;
using Next_Future_ERP.Features.Permissions.Views;

using Next_Future_ERP.Features.PurchaseInvoices.Models;
using Next_Future_ERP.Features.PurchaseInvoices.Services;
using Next_Future_ERP.Features.PurchaseInvoices.ViewModels;
using Next_Future_ERP.Features.PurchaseInvoices.Views;
using Next_Future_ERP.Features.Purchases.Models;

using Next_Future_ERP.Features.PrintManagement.Services;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Suppliers.ViewModels;
using Next_Future_ERP.Features.Warehouses.Services;
using Next_Future_ERP.Features.Warehouses.ViewModels;
using Next_Future_ERP.Features.Warehouses.Views;

using Next_Future_ERP.Features.Purchases.Services;
using Next_Future_ERP.Features.Purchases.ViewModels;
using Next_Future_ERP.Features.Purchases.Views;
using Next_Future_ERP.Features.StoreReceipts.Models;
using Next_Future_ERP.Features.StoreReceipts.Services;
using Next_Future_ERP.Features.StoreReceipts.ViewModels;
using Next_Future_ERP.Features.StoreReceipts.Views;
using Next_Future_ERP.Features.StoreIssues.Models;
using Next_Future_ERP.Features.StoreIssues.Services;
using Next_Future_ERP.Features.StoreIssues.ViewModels;
using Next_Future_ERP.Features.Suppliers.Views;

using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using Next_Future_ERP.Features.Accounts.ViewModels;
using Next_Future_ERP.Features.StoreIssues.Views;
using Next_Future_ERP.Features.StoreIssues.Models;
using Next_Future_ERP.Features.PosStations.Services;
using Next_Future_ERP.Features.PosStations.ViewModels;
using Next_Future_ERP.Features.PosStations.Views;
using Next_Future_ERP.Features.PosOperators.Services;
using Next_Future_ERP.Features.PosOperators.ViewModels;
using Next_Future_ERP.Features.PosOperators.Views;

namespace Next_Future_ERP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;
        public static IServiceProvider ServiceProvider { get; private set; }
        public static MainWindowModel? MainViewModel { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            ServiceProvider = _host.Services;

            // Initialize MainViewModel using DI
            MainViewModel = ServiceProvider.GetRequiredService<MainWindowModel>();

            // Create and show the login window using DI
            var loginView = ServiceProvider.GetRequiredService<LoginView>();
            loginView.Show();

            base.OnStartup(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register DbContext with proper lifetime management for WPF
            // اختَر Lifetime مناسب لبيئة WPF/سطح المكتب:
            // Transient آمن عادةً حتى لا يعيش DbContext طويلًا على نفس الثريد/النافذة.
            services.AddDbContext<AppDbContext>(options =>
            {
                var settings = Data.Services.SettingsService.Load();
                var connectionString = BuildConnectionString(settings);

                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(60);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            }, contextLifetime: ServiceLifetime.Transient);

            // Register Authentication Service
            services.AddScoped<IAuthService, AuthService>();
            services.AddSingleton<ISessionService, SessionService>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<LoginView>();
            services.AddTransient<MainWindowModel>();
            services.AddScoped<IDataSourceExecutor, SqlDataSourceExecutor>();
            services.AddScoped<IRenderPreviewService, RenderPreviewService>();

            // Register Category services

            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<CategoriesViewModel>();

            // Register Units services
            services.AddTransient<IUnitsService, UnitsService>();
            services.AddTransient<UnitsViewModel>();

            // === Payment & Receipt Vouchers ===
            services.AddTransient<Next_Future_ERP.Features.Accounts.Services.PaymentVoucherService>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.PaymentVoucherViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.PaymentVoucherListViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.PaymentVoucherEditViewModel>();
            
            services.AddTransient<Next_Future_ERP.Features.Accounts.Services.ReceiptVoucherService>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.ReceiptVoucherViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.ReceiptVoucherListViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.ReceiptVoucherEditViewModel>();

            // === Payment Terms ===
            services.AddTransient<IPaymentTermsService, PaymentTermsService>();
            services.AddTransient<PaymentTermsViewModel>();

            services.AddTransient<IPaymentTypesService, PaymentTypesService>();
            services.AddTransient<PaymentTypesViewModel>();

            services.AddTransient<IPaymentMethodsService, PaymentMethodsService>();
            services.AddTransient<PaymentMethodsViewModel>();

            services.AddTransient<ISuppliersService, SuppliersService>();
            services.AddTransient<SuppliersViewModel>();
            services.AddTransient<SupplierLookupViewModel>();
            services.AddTransient<SupplierLookupWindow>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.Services.NextCurrencyService>();

            // Register Command Services
            services.AddSingleton<Next_Future_ERP.Core.Services.PermissionService>();
            services.AddSingleton<Next_Future_ERP.Core.Services.CommandService>();

            // Register Account Services
            services.AddTransient<IAccountClassService, AccountClassService>();
            services.AddTransient<AccountClassesViewModel>();
            
            services.AddTransient<ICostCentersService, CostCentersService>();
            services.AddTransient<CostCentersViewModel>();
            
            // Banks Services
            services.AddTransient<Next_Future_ERP.Features.Accounts.Services.BankService>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.BanksListViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.BankEditViewModel>();
            
            // Funds Services
            services.AddTransient<Next_Future_ERP.Features.Accounts.Services.FundService>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.FundsListViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.FundEditViewModel>();
            
            // Opening Balance Services
            services.AddTransient<Next_Future_ERP.Features.Accounts.Services.IReferenceDataService, Next_Future_ERP.Features.Accounts.Services.ReferenceDataService>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.Services.IOpeningBalanceService, Next_Future_ERP.Features.Accounts.Services.OpeningBalanceService>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.OpeningBalanceListViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.OpeningBalanceEditViewModel>();
            
            services.AddTransient<INextCurrencyService, NextCurrencyService>();
            services.AddTransient<NextCurrenciesViewModel>();
            
            services.AddTransient<ICurrencyExchangeRateService, CurrencyExchangeRateService>();
            services.AddTransient<CurrencyExchangeRatesViewModel>();

            services.AddTransient<IWarehouseService, WarehouseService>();
            services.AddTransient<WarehouseService>();
            services.AddTransient<IOrgLookupService, OrgLookupService>();
            services.AddTransient<WarehousesViewModel>();
            services.AddTransient<WarehouseEditViewModel>();
            
            // Valuation Group Services
            services.AddTransient<IValuationGroupService, ValuationGroupService>();
            services.AddTransient<ValuationGroupViewModel>();
            services.AddTransient<ValuationGroupEditViewModel>();
        
           
            // === Opening Balance Services ===
            services.AddTransient<IReferenceDataService,ReferenceDataService>();
            services.AddTransient<IOpeningBalanceService,OpeningBalanceService>();

            services.AddTransient<OpeningBalanceImportService>();

            //                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      services.AddTransient<IOpeningBalanceImportService, OpeningBalanceImportService>();

            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.OpeningBalanceViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.Views.PaymentVoucherListView>();

            // === Debit Credit Notification Services ===
            services.AddTransient<Next_Future_ERP.Features.Accounts.Services.DebitCreditNotificationService>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.DebitCreditNotificationViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.DebitCreditNotificationListViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.ViewModels.DebitCreditNotificationEditViewModel>();
            services.AddTransient<Next_Future_ERP.Features.Accounts.Views.DebitCreditNotificationListView>();


            // === Print Management Services ===
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Services.ITemplateCatalogService, Next_Future_ERP.Features.PrintManagement.Services.TemplateCatalogService>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Services.IVersioningService, Next_Future_ERP.Features.PrintManagement.Services.VersioningService>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Services.IContentService, Next_Future_ERP.Features.PrintManagement.Services.ContentService>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Services.IRenderPreviewService, Next_Future_ERP.Features.PrintManagement.Services.RenderPreviewService>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Services.PrintManagementInitializationService>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Services.PrintManagementSeedDataService>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.ViewModels.TemplateLibraryViewModel>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.ViewModels.TemplateWorkspaceViewModel>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Views.TemplateLibraryView>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Views.TemplateWorkspaceView>();

            // Inventory Opening Services
            services.AddTransient<ItemsViewModel>();
            services.AddTransient<ItemEditViewModel>();
           
            services.AddTransient<ItemPricesViewModel>();
            services.AddTransient<IItemsService, ItemsService>();
            services.AddTransient<IItemTypeService, ItemTypeService>(); // لو ItemsService يحتاجها
            services.AddTransient<IItemCostsService, ItemCostsService>(); // لو ItemsService يحتاجها
            services.AddTransient<IItemSuppliersService, ItemSuppliersService>(); // لو ItemsService يحتاجها
            services.AddTransient<IItemBatchesService, ItemBatchesService>(); // لو ItemsService يحتاجها
            services.AddTransient<IItemComponentsService, ItemComponentsService>(); // لو ItemsService يحتاجها
            services.AddTransient<IUnitsService, UnitsService>();       // إذا الـVM يستعمل الوحدات
            services.AddTransient<ICategoryService, CategoryService>(); // إن وجِد
         
            services.AddTransient<OrgLookupService>();  // ← الجديد
            services.AddTransient<IWarehouseService, WarehouseService>();  // ← الجديد
            services.AddTransient<WarehouseService>();
            services.AddTransient<IItemPricesService, ItemPricesService>();  // ← الجديد
            services.AddTransient<ItemCostsViewModel>();  // ← الجديد
            services.AddTransient<ItemSuppliersViewModel>();  // ← الجديد
            services.AddTransient<ItemBatchesViewModel>();  // ← الجديد
            services.AddTransient<ItemComponentsViewModel>();  // ← الجديد
            services.AddTransient<IInventoryBalanceService, InventoryBalanceService>();  // ← الجديد
            services.AddTransient<InventoryBalanceViewModel>();  // ← الجديد

            // Dashboard Services
            services.AddTransient<IPurchaseDashboardService, PurchaseDashboardService>();
            services.AddTransient<PurchaseDashboardViewModel>();


            services.AddTransient<InventoryOpeningView>();
            services.AddTransient<InventoryOpeningBrowseViewModel>();
            services.AddTransient<InventoryOpeningBrowseView>();
            services.AddTransient<ISuppliersService, SuppliersService>();
            services.AddTransient<IUnitsLookupService, UnitsLookupService>();
            services.AddTransient<ICategoriesLookupService, CategoriesLookupService>();
            services.AddTransient<IUnitsService, UnitsService>();
            services.AddTransient<IPurchaseTxnsService, PurchaseTxnsService>();

            // === Store Receipts Services ===
            services.AddTransient<IStoreReceiptsService, StoreReceiptsService>();
            services.AddTransient<StoreReceiptListViewModel>(sp => 
                new StoreReceiptListViewModel(
                    sp.GetRequiredService<IStoreReceiptsService>(),
                    sp.GetRequiredService<ISuppliersService>()));
        services.AddTransient<StoreReceiptEditViewModel>(sp =>
            new StoreReceiptEditViewModel(
                sp.GetRequiredService<IStoreReceiptsService>(),
                sp.GetRequiredService<ISuppliersService>(),
                sp.GetRequiredService<IItemsService>(),
                sp.GetRequiredService<IUnitsService>(),
                sp.GetRequiredService<IWarehouseService>(),
                sp.GetRequiredService<IPurchaseTxnsService>(),
                new StoreReceipt()));

            // === ViewModels factories ===
            services.AddTransient<PurchaseListViewModel>();
            services.AddTransient<Func<char, PurchaseListViewModel>>(sp => t =>
     new PurchaseListViewModel(
         sp.GetRequiredService<IPurchaseTxnsService>(),
         sp.GetRequiredService<ISuppliersService>(),  // 👈 أضفها هنا
         t));


            services.AddTransient<Func<char, PurchaseTxn, PurchaseEditViewModel>>(sp => (t, m) =>
                new PurchaseEditViewModel(
                    sp.GetRequiredService<IPurchaseTxnsService>(),
                    sp.GetRequiredService<ISuppliersService>(),
                    sp.GetRequiredService<IItemsService>(),     // 👈 الآن متوفرة
                    sp.GetRequiredService<IUnitsService>(),
                    t, m));

            // === Views (اختياري) ===
            services.AddTransient<PurchaseOrdersListView>();   // يرث من PurchaseListView : base('P')
            services.AddTransient<PurchaseReturnsListView>();
            
            // === Store Receipts Views ===
            services.AddTransient<StoreReceiptListView>();
            services.AddTransient<StoreReceiptEditWindow>();
            services.AddTransient<Features.StoreReceipts.Views.PurchaseOrderSelectionWindow>();

            // === Store Issues Services ===
            services.AddTransient<IIssueDestinationsService, IssueDestinationsService>();
            services.AddTransient<AccountsService>();
            services.AddTransient<IssueDestinationsViewModel>(sp => 
                new IssueDestinationsViewModel(
                    sp.GetRequiredService<IIssueDestinationsService>(),
                    sp.GetRequiredService<AccountsService>(),
                    sp.GetRequiredService<ICostCentersService>()));
            services.AddTransient<IssueDestinationsView>();

            // === Store Issues (الصرف المخزني) Services ===
            services.AddTransient<IStoreIssuesService, StoreIssuesService>();
            services.AddTransient<StoreIssuesViewModel>();
            services.AddTransient<StoreIssuesView>();
            services.AddTransient<Func<StoreIssue, StoreIssueEditViewModel>>(sp => model =>
                new StoreIssueEditViewModel(
                    model,
                    sp.GetRequiredService<IStoreIssuesService>(),
                    sp.GetRequiredService<IIssueDestinationsService>()));
            services.AddTransient<Func<StoreIssue, StoreIssueEditWindow>>(sp => model =>
                new StoreIssueEditWindow(
                    model,
                    sp.GetRequiredService<IStoreIssuesService>()));

            // === Purchase Invoices Services ===
            services.AddTransient<IPurchaseAPService, PurchaseAPService>();
            services.AddTransient<PurchaseAPListViewModel>(sp => 
                new PurchaseAPListViewModel(
                    sp.GetRequiredService<IPurchaseAPService>(),
                    sp.GetRequiredService<ISuppliersService>()));

            // === Purchase Invoices Views ===
            services.AddTransient<PurchaseAPListView>();
            services.AddTransient<PurchaseAPEditWindow>();
            services.AddTransient<StoreReceiptSelectionWindow>();
            services.AddTransient<PurchaseAPEditViewModel>(sp =>
                new PurchaseAPEditViewModel(
                    sp.GetRequiredService<IPurchaseAPService>(),
                    sp.GetRequiredService<ISuppliersService>(),
                    sp.GetRequiredService<IItemsService>(),
                    sp.GetRequiredService<IUnitsService>(),
                    sp.GetRequiredService<IWarehouseService>(),
                    sp.GetRequiredService<IStoreReceiptsService>(),
                    sp.GetRequiredService<IPurchaseTxnsService>(),
                    new PurchaseAP()));

            // Permissions System
            services.AddTransient<IPermissionService, PermissionService>();
            services.AddTransient<PermissionsMainViewModel>();
            services.AddTransient<MenuEditorViewModel>();
            services.AddTransient<PermissionsMainView>();

            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Services.ITemplateCatalogService, Next_Future_ERP.Features.PrintManagement.Services.TemplateCatalogService>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Services.IVersioningService, Next_Future_ERP.Features.PrintManagement.Services.VersioningService>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Services.IContentService, Next_Future_ERP.Features.PrintManagement.Services.ContentService>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Services.IRenderPreviewService, Next_Future_ERP.Features.PrintManagement.Services.RenderPreviewService>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.ViewModels.TemplateLibraryViewModel>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.ViewModels.TemplateWorkspaceViewModel>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Views.TemplateLibraryView>();
            services.AddTransient<Next_Future_ERP.Features.PrintManagement.Views.TemplateWorkspaceView>();

            // Sales Services
            services.AddTransient<Next_Future_ERP.Features.PosStations.Services.IPosStationService, Next_Future_ERP.Features.PosStations.Services.PosStationService>();
            services.AddTransient<Next_Future_ERP.Features.PosStations.ViewModels.PosStationsListViewModel>();
            services.AddTransient<Next_Future_ERP.Features.PosStations.ViewModels.PosStationEditViewModel>();
            services.AddTransient<Next_Future_ERP.Features.PosStations.Views.PosStationsListView>();
            services.AddTransient<Next_Future_ERP.Features.PosStations.Views.PosStationEditWindow>();

            // PosOperators Services
            services.AddTransient<Next_Future_ERP.Features.PosOperators.Services.IPosOperatorService, Next_Future_ERP.Features.PosOperators.Services.PosOperatorService>();
            services.AddTransient<Next_Future_ERP.Features.PosOperators.ViewModels.PosOperatorsListViewModel>();
            services.AddTransient<Next_Future_ERP.Features.PosOperators.ViewModels.PosOperatorEditViewModel>();
            services.AddTransient<Next_Future_ERP.Features.PosOperators.Views.PosOperatorsListView>();
            services.AddTransient<Next_Future_ERP.Features.PosOperators.Views.PosOperatorEditWindow>();

            // SystemUsers Services
            services.AddTransient<Next_Future_ERP.Features.SystemUsers.Services.ISystemUserService, Next_Future_ERP.Features.SystemUsers.Services.SystemUserService>();
            services.AddTransient<Next_Future_ERP.Features.SystemUsers.ViewModels.SystemUsersListViewModel>();
            services.AddTransient<Next_Future_ERP.Features.SystemUsers.ViewModels.SystemUserEditViewModel>();
            services.AddTransient<Next_Future_ERP.Features.SystemUsers.Views.SystemUsersListView>();
            services.AddTransient<Next_Future_ERP.Features.SystemUsers.Views.SystemUserEditWindow>();

        }

        private static string BuildConnectionString(Data.Models.ConnectionSettings settings)
        {
            var server = settings.Server;
            if (string.IsNullOrWhiteSpace(server))
            {
                server = @"localhost";
            }

            if (settings.Port.HasValue)
            {
                server = $"{server},{settings.Port.Value}";
            }

            var database = string.IsNullOrWhiteSpace(settings.Database) ? "NextFutureERP" : settings.Database;

            var type = (settings.Type ?? "Server").Trim();
            var isIntegrated = string.Equals(type, "Local", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(type, "Locals", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(type, "Windows", StringComparison.OrdinalIgnoreCase);

        


            var common = ";TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;Command Timeout=60;Pooling=true;Max Pool Size=100;Min Pool Size=0";

            if (isIntegrated)
            {
                return $"Server={server};Database={database};Integrated Security=True{common}";
            }

            var user = settings.Username ?? string.Empty;
            var pass = settings.Password ?? string.Empty;
            return $"Server={server};Database={database};User Id={user};Password={pass}{common}";
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}
