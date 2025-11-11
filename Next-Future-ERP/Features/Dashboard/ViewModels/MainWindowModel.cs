using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Accounts.Views;
using Next_Future_ERP.Features.Auth.Services;
using Next_Future_ERP.Features.Dashboard.Models;
using Next_Future_ERP.Features.Dashboard.Views;
using Next_Future_ERP.Features.Inventory.Views;
using Next_Future_ERP.Features.Items.Views;
using Next_Future_ERP.Features.Payments.Views;
using Next_Future_ERP.Features.Permissions.Views;
using Next_Future_ERP.Features.PrintManagement.Views;
using Next_Future_ERP.Features.PurchaseInvoices.Views;
using Next_Future_ERP.Features.Purchases.ViewModels;
using Next_Future_ERP.Features.StoreIssues.Views;
using Next_Future_ERP.Features.StoreReceipts.Views;
using Next_Future_ERP.Features.StoreSetting.Views;
using Next_Future_ERP.Features.Suppliers.Views;
using Next_Future_ERP.Features.Warehouses.Views;
using Next_Future_ERP.Features.PosStations.Views;
using Next_Future_ERP.Features.PosOperators.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Next_Future_ERP.Features.SystemUsers.Views;

namespace Next_Future_ERP.Features.Dashboard.ViewModels
{
    // عنصر قائمة تنقل هرمي
    public partial class MenuItemVm : ObservableObject
    {
        public string Title { get; set; } = "";
        public string Icon { get; set; } = ""; // مسار الأيقونة أو رمز الأيقونة
        public Type? PageType { get; set; } // نوع الصفحة/العنصر المراد فتحه
        public int? FormId { get; set; } // معرف النموذج للتحقق من الصلاحيات
        public ObservableCollection<MenuItemVm> Children { get; set; } = new();
        
        [ObservableProperty]
        private bool isExpanded = false; // حالة التوسيع/الطي - مطوي بشكل افتراضي
        
        [ObservableProperty]
        private bool isSelected = false; // حالة التحديد
        
        [ObservableProperty]
        private bool isVisible = true; // حالة الظهور بناءً على الصلاحيات
    }

    public partial class MainWindowModel : ObservableObject
    {
        private readonly ISessionService _sessionService;

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private bool isMenuCollapsed = false; // حالة إخفاء/إظهار أسماء القائمة

        public string ApplicationTitle => "Next Future ERP";
        public string CurrentUserName => _sessionService.CurrentUser?.FullName ?? _sessionService.CurrentUser?.Name ?? "مستخدم غير معروف";
        public string CurrentUserInfo => $"{CurrentUserName} - {_sessionService.CurrentUser?.CompanyName ?? "شركة غير محددة"}";
        public bool IsUserLoggedIn => _sessionService.CurrentUser != null;

        // القائمة الهرمية (يمين)
        [ObservableProperty] private ObservableCollection<MenuItemVm> menu = new();

        // التبويبات المفتوحة
        [ObservableProperty] private ObservableCollection<TabItem> openTabs = new();

        [ObservableProperty] private TabItem? selectedTab;

        public IRelayCommand<MenuItemVm> OpenFromMenuCommand { get; }
        public IRelayCommand<TabItem> CloseTabCommand { get; }
        public IRelayCommand CloseAllTabsCommand { get; }
        public IRelayCommand<TabItem> SelectTabCommand { get; }
        public IRelayCommand ToggleMenuCollapseCommand { get; }

        public MainWindowModel(ISessionService sessionService)
        {
            _sessionService = sessionService;

            OpenFromMenuCommand = new RelayCommand<MenuItemVm>(OpenFromMenu);
            CloseTabCommand = new RelayCommand<TabItem>(CloseTab);
            CloseAllTabsCommand = new RelayCommand(CloseAllTabs, () => OpenTabs.Any(t => t.CanClose));
            SelectTabCommand = new RelayCommand<TabItem>(SelectTab);
            ToggleMenuCollapseCommand = new RelayCommand(ToggleMenuCollapse);

            BuildMenu(); // يبني القائمة
            
            // فتح صفحة الرئيسية بشكل افتراضي عند بدء تشغيل النظام
            OpenDefaultTab();
            
            _sessionService.SessionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(CurrentUserName));
                OnPropertyChanged(nameof(CurrentUserInfo));
                OnPropertyChanged(nameof(IsUserLoggedIn));
                
                // تحديث صلاحيات القائمة عند تغيير الجلسة
                UpdateMenuPermissions();
            };
        }

        private void BuildMenu()
        {
            // Menu structure with Form IDs from PermissionSeedData.GetDefaultMenuForms()
            var root = new ObservableCollection<MenuItemVm>
            {
                // FormId 1: Dashboard
                new MenuItemVm { 
                    Title = "الرئيسية", 
                    Icon = "M10,20V14H14V20H19V12H22L12,3L2,12H5V20H10Z", 
                    PageType = typeof(DashboardView),
                    FormId = 1 
                },
                // FormId 2: Accounts (Parent) - "الترميزات"
                new MenuItemVm {
                    Title = "الترميزات",
                    Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z",
                    FormId = 2,
                    Children = {
                        new MenuItemVm { Title = "تعريف الحسابات", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Accounts.Views.AccountsView), FormId = 21 },
                        new MenuItemVm { Title = "تصنيف الحسابات", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Accounts.Views.AccountClassesListView), FormId = 21 },
                        new MenuItemVm { Title = "مراكز التكلفة", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Accounts.Views.CostCentersListView), FormId = 21 },
                        new MenuItemVm { Title = "ترميز العملات", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Accounts.Views.NextCurrenciesView), FormId = 25 },
                        new MenuItemVm { Title = "معامل تحويل العملات", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Accounts.Views.CurrencyExchangeRatesView), FormId = 25 },
                        new MenuItemVm { Title = "أنواع المستندات", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Accounts.Views.DocumentTypesView), FormId = 21 },
                        new MenuItemVm { Title = "البنوك", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(BanksListView), FormId = 26 },
                        new MenuItemVm { Title = "تعريف الصناديق", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(FundsListView), FormId = 21 },
                        new MenuItemVm { Title = "البيانات الضريبية للشركة", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(CompanyTaxProfileView), FormId = 21 },
                        new MenuItemVm { Title = "ترميز المستخدمين", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(SystemUsersListView), FormId = 27 },
                    }
                },

                    // Operations (العمليات) - Related to Accounts
                new MenuItemVm {
                    Title = "العمليات",
                    Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z",
                    Children = {
                        new MenuItemVm { Title = "الأرصدة الافتتاحية", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(OpeningBalanceListView), FormId = 21 },
                        new MenuItemVm { Title = "سندات الصرف", Icon = "M9,3V4H4V6H5V19A2,2 0 0,0 7,21H17A2,2 0 0,0 19,19V6H20V4H15V3H9M7,6H17V19H7V6M9,8V17H11V8H9M13,8V17H15V8H13Z", PageType = typeof(PaymentVoucherListView), FormId = 23 },
                        new MenuItemVm { Title = "سندات القبض", Icon = "M9,3V4H4V6H5V19A2,2 0 0,0 7,21H17A2,2 0 0,0 19,19V6H20V4H15V3H9M7,6H17V19H7V6M9,8V17H11V8H9M13,8V17H15V8H13Z", PageType = typeof(ReceiptVoucherListView), FormId = 24 },
                        new MenuItemVm { Title = "الإشعارات المدينة والدائنة", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(DebitCreditNotificationListView), FormId = 21 },
                        new MenuItemVm { Title = "قيد يومية عامة", Icon = "M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z", PageType = typeof(GeneralJournalEntryView), FormId = 22 },
                    }
                },

                   // FormId 3: Inventory (Parent)
                new MenuItemVm {
                    Title = "المخزون",
                    Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z",
                    FormId = 3,
                    Children = {
                        new MenuItemVm { Title = "تعريف الأصناف", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(ItemsView), FormId = 31 },
                        new MenuItemVm { Title = "إدارة الفئات", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(CategoriesView), FormId = 32 },
                        new MenuItemVm { Title = "إدارة وحدات القياس", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(UnitsPage), FormId = 33 },
                        new MenuItemVm { Title = "تعريف المخازن", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(WarehousesView), FormId = 34 },
                        new MenuItemVm { Title = "تقييم المخزون", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(ValuationGroupView), FormId = 31 },
                        new MenuItemVm { Title = "إعدادات المخزون", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(StoreMainV), FormId = 31 },
                        new MenuItemVm { Title = "تعريف الأسعار", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(ItemPricesView), FormId = 31 },
                        new MenuItemVm { Title = "الرصيد الافتتاحي", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(InventoryOpeningView), FormId = 31 },
                    }
                },

                  // FormId 5: Purchases (Parent)
                new MenuItemVm {
                    Title = "المشتريات",
                    Icon = "M19,7H16V6A2,2 0 0,0 14,4H10A2,2 0 0,0 8,6V7H5A1,1 0 0,0 4,8V19A3,3 0 0,0 7,22H17A3,3 0 0,0 20,19V8A1,1 0 0,0 19,7M10,6H14V7H10V6M18,19A1,1 0 0,1 17,20H7A1,1 0 0,1 6,19V9H18V19Z",
                    FormId = 5,
                    Children = {
                        new MenuItemVm { Title = "لوحة مشتريات", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(PurchaseDashboardView), FormId = 5 },
                        new MenuItemVm { Title = "أوامر الشراء", Icon = "M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z", PageType = typeof(PurchaseOrdersListView), FormId = 51 },
                        new MenuItemVm { Title = "سندات الفحص والاستلام", Icon = "M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z", PageType = typeof(StoreReceiptListView), FormId = 55 },
                        new MenuItemVm { Title = "مرتجعات الشراء", Icon = "M9,3V4H4V6H5V19A2,2 0 0,0 7,21H17A2,2 0 0,0 19,19V6H20V4H15V3H9M7,6H17V19H7V6M9,8V17H11V8H9M13,8V17H15V8H13Z", PageType = typeof(PurchaseReturnsListView), FormId = 53 },
                        new MenuItemVm { Title = "فواتير المشتريات", Icon = "M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z", PageType = typeof(PurchaseAPListView), FormId = 52 },
                        new MenuItemVm { Title = "الصرف المخزني", Icon = "M9,3V4H4V6H5V19A2,2 0 0,0 7,21H17A2,2 0 0,0 19,19V6H20V4H15V3H9M7,6H17V19H7V6M9,8V17H11V8H9M13,8V17H15V8H13Z", PageType = typeof(StoreIssuesView), FormId = 55 },
                        new MenuItemVm { Title = "الموردين", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(SuppliersListView), FormId = 54 },
                        new MenuItemVm { Title = "جهات الصرف", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(IssueDestinationsView), FormId = 55 },
                    }
                },

                // FormId 6: Sales (Parent) - المبيعات
                new MenuItemVm {
                    Title = "المبيعات",
                    Icon = "M19,7H16V6A2,2 0 0,0 14,4H10A2,2 0 0,0 8,6V7H5A1,1 0 0,0 4,8V19A3,3 0 0,0 7,22H17A3,3 0 0,0 20,19V8A1,1 0 0,0 19,7M10,6H14V7H10V6M18,19A1,1 0 0,1 17,20H7A1,1 0 0,1 6,19V9H18V19Z",
                    FormId = 6,
                    Children = {
                        new MenuItemVm { Title = "لوحة المبيعات", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Sales.Views.SalesDashboardView), FormId = 6 },
                        new MenuItemVm { Title = "أوامر البيع", Icon = "M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z", PageType = typeof(Features.Sales.Views.SalesOrdersListView), FormId = 61 },
                        new MenuItemVm { Title = "فواتير المبيعات", Icon = "M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z", PageType = typeof(Features.Sales.Views.SalesInvoicesListView), FormId = 62 },
                        new MenuItemVm { Title = "مرتجعات المبيعات", Icon = "M9,3V4H4V6H5V19A2,2 0 0,0 7,21H17A2,2 0 0,0 19,19V6H20V4H15V3H9M7,6H17V19H7V6M9,8V17H11V8H9M13,8V17H15V8H13Z", PageType = typeof(Features.Sales.Views.SalesReturnsListView), FormId = 63 },
                        new MenuItemVm { Title = "العملاء", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Sales.Views.CustomersListView), FormId = 64 },
                        new MenuItemVm { Title = "نقطة البيع", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Sales.Views.PosView), FormId = 65 },
                        new MenuItemVm { Title = "إعدادات المبيعات", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Sales.Views.SalesSettingsView), FormId = 66 },
                        new MenuItemVm { Title = "إعدادات نقاط البيع", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.Sales.Views.PosSettingsView), FormId = 67 },
                        new MenuItemVm { Title = "محطات نقاط البيع", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.PosStations.Views.PosStationsListView), FormId = 68 },
                        new MenuItemVm { Title = "مشغلي نقاط البيع", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(Features.PosOperators.Views.PosOperatorsListView), FormId = 69 },
                    }
                },
           
                // Payment (الدفع) - No direct mapping in seed data, using general access
                new MenuItemVm {
                    Title = "الدفع",
                    Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z",
                    Children = {
                        new MenuItemVm { Title = "شروط الدفع", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(PaymentTermsView), FormId = 21 },
                        new MenuItemVm { Title = "أنواع الدفع", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(PaymentTypesView), FormId = 21 },
                        new MenuItemVm { Title = "طرق الدفع", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(PaymentMethodsView), FormId = 21 },
                    }
                },

                // Print Management - Related to Settings FormId 7
                new MenuItemVm {
                    Title = "إدارة الطابعة والمستندات",
                    Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z",
                    Children = {
                        new MenuItemVm { Title = "تهيئة الطابعة والمستندات", Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", PageType = typeof(TemplateLibraryView), FormId = 71 },
                    }
                },

                // FormId 7: Settings (Permissions Management)
                new MenuItemVm { 
                    Title = "إدارة الصلاحيات", 
                    Icon = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", 
                    PageType = typeof(PermissionsMainView),
                    
                },
            };

            Menu = root;
            
            // فحص الصلاحيات وتحديث حالة الظهور لكل عنصر
            UpdateMenuPermissions();
        }

        /// <summary>
        /// فحص صلاحيات القائمة وتحديث حالة الظهور
        /// </summary>
        private void UpdateMenuPermissions()
        {
            foreach (var menuItem in Menu)
            {
                UpdateMenuItemPermission(menuItem);
            }
        }

        /// <summary>
        /// فحص صلاحيات عنصر القائمة والعناصر الفرعية
        /// </summary>
        /// <param name="item">عنصر القائمة</param>
        private void UpdateMenuItemPermission(MenuItemVm item)
        {
            // إذا كان العنصر يحتوي على FormId، نفحص الصلاحيات
            if (item.FormId.HasValue)
            {
                item.IsVisible = _sessionService.CanAccessForm(item.FormId.Value);
            }
            else
            {
                // إذا لم يكن هناك FormId، نفحص إذا كان له عناصر فرعية مرئية
                if (item.Children.Count > 0)
                {
                    // فحص صلاحيات العناصر الفرعية أولاً
                    foreach (var child in item.Children)
                    {
                        UpdateMenuItemPermission(child);
                    }
                    
                    // العنصر الأب يظهر فقط إذا كان له عنصر فرعي واحد على الأقل مرئي
                    item.IsVisible = item.Children.Any(c => c.IsVisible);
                }
                else
                {
                    // إذا لم يكن هناك FormId ولا عناصر فرعية، يظهر افتراضياً
                    item.IsVisible = true;
                }
            }
        }

        private void OpenFromMenu(MenuItemVm? item)
        {
            if (item?.PageType == null) return;

            // هل التبويب مفتوح مسبقاً؟
            var existing = OpenTabs.FirstOrDefault(t => t.PageType == item.PageType);
            if (existing != null) { SelectTab(existing); return; }

            // افتح تبويب جديد
            var title = item.Title;
            var content = CreateContentFor(item.PageType);
            if (content == null)
            {
                MessageBox.Show($"تعذّر تحميل المحتوى للصفحة: {item.PageType.Name}");
                return;
            }

            var tab = new TabItem(title, item.PageType, content);
            OpenTabs.Add(tab);
            SelectTab(tab);
            CloseAllTabsCommand.NotifyCanExecuteChanged();
        }

        private FrameworkElement? CreateContentFor(Type pageType)
        {
            try
            {
                FrameworkElement? element = null;

                // جرّب عبر DI أولاً
                if (App.ServiceProvider != null)
                {
                    var instance = App.ServiceProvider.GetService(pageType);
                    if (instance is FrameworkElement fe) element = fe;
                }

                // لو فشل DI استخدم Activator
                if (element == null)
                {
                    var inst = Activator.CreateInstance(pageType);
                    if (inst is FrameworkElement feA)
                        element = feA;
                }

                if (element == null) return null;

                // لو كان Page، لفّه داخل Frame منفصل لهذا التبويب
                if (element is System.Windows.Controls.Page p)
                {
                    var frame = new System.Windows.Controls.Frame
                    {
                        NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                        JournalOwnership = System.Windows.Navigation.JournalOwnership.OwnsJournal
                    };
                    frame.Navigate(p);
                    return frame;
                }

                // غير ذلك UserControl/Element جاهز
                return element;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء إنشاء المحتوى: {ex.Message}");
                return null;
            }
        }

        private void CloseTab(TabItem? tab)
        {
            if (tab == null || !tab.CanClose) return;

            var index = OpenTabs.IndexOf(tab);
            var wasSelected = (SelectedTab == tab);

            OpenTabs.Remove(tab);

            if (wasSelected)
            {
                if (OpenTabs.Count > 0)
                {
                    var newIndex = Math.Min(index, OpenTabs.Count - 1);
                    SelectTab(OpenTabs[newIndex]);
                }
                else
                {
                    SelectedTab = null;
                }
            }

            CloseAllTabsCommand.NotifyCanExecuteChanged();
        }

        private void CloseAllTabs()
        {
            foreach (var t in OpenTabs.Where(t => t.CanClose).ToList())
                OpenTabs.Remove(t);

            SelectedTab = null;
            CloseAllTabsCommand.NotifyCanExecuteChanged();
        }

        private void SelectTab(TabItem tab)
        {
            foreach (var t in OpenTabs) t.IsSelected = (t == tab);
            SelectedTab = tab;
            OnPropertyChanged(nameof(SelectedTab));
        }

        private void OpenDefaultTab()
        {
            // البحث عن عنصر "الرئيسية" في القائمة
            var homeItem = Menu.FirstOrDefault(m => m.Title == "الرئيسية");
            if (homeItem != null)
            {
                // فتح صفحة الرئيسية بشكل افتراضي
                OpenFromMenu(homeItem);
            }
        }

        private void ToggleMenuCollapse()
        {
            IsMenuCollapsed = !IsMenuCollapsed;
        }
    }
}
