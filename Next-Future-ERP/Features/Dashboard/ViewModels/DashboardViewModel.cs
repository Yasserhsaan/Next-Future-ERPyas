using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Auth.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Next_Future_ERP.Features.Dashboard.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly ISessionService _sessionService;

    [ObservableProperty]
    private string welcomeMessage = "مرحباً بك في نظام إدارة الموارد المؤسسية";

    [ObservableProperty]
    private decimal totalSales;

    [ObservableProperty]
    private decimal totalPurchases;

    [ObservableProperty]
    private int totalCustomers;

    [ObservableProperty]
    private int totalSuppliers;

    [ObservableProperty]
    private ObservableCollection<NotificationItem> notifications = new();

    [ObservableProperty]
    private ObservableCollection<TaskItem> pendingTasks = new();

    public DashboardViewModel()
    {
        // Constructor للـ XAML
        _context = null!;
        _sessionService = null!;
        
        // تحميل البيانات التجريبية
        LoadSampleData();
    }

    public DashboardViewModel(AppDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
        
        InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadStatisticsAsync();
        await LoadNotificationsAsync();
        await LoadPendingTasksAsync();
        UpdateWelcomeMessage();
    }

    private void LoadSampleData()
    {
        // تحميل بيانات تجريبية للعرض
        TotalSales = 1250000.50m;
        TotalPurchases = 890000.75m;
        TotalCustomers = 156;
        TotalSuppliers = 45;
        
        WelcomeMessage = "مرحباً بك في نظام Next Future ERP";
        
        LoadSampleNotifications();
        LoadSampleTasks();
    }

    private void LoadSampleNotifications()
    {
        var notificationsList = new List<NotificationItem>
        {
            new NotificationItem
            {
                Title = "تنبيه مهم",
                Message = "يوجد 5 فواتير مبيعات معلقة تحتاج للمراجعة",
                Date = DateTime.Now.AddHours(-2).ToString("dd/MM/yyyy HH:mm"),
                Type = NotificationType.Warning
            },
            new NotificationItem
            {
                Title = "تذكير",
                Message = "موعد استحقاق فاتورة مورد رقم INV-2024-001 غداً",
                Date = DateTime.Now.AddHours(-1).ToString("dd/MM/yyyy HH:mm"),
                Type = NotificationType.Info
            },
            new NotificationItem
            {
                Title = "تحذير",
                Message = "مخزون الصنف ABC-123 منخفض (أقل من الحد الأدنى)",
                Date = DateTime.Now.AddMinutes(-30).ToString("dd/MM/yyyy HH:mm"),
                Type = NotificationType.Danger
            }
        };

        Notifications.Clear();
        foreach (var notification in notificationsList)
        {
            Notifications.Add(notification);
        }
    }

    private void LoadSampleTasks()
    {
        var tasksList = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "مراجعة فواتير المبيعات",
                Description = "مراجعة 5 فواتير مبيعات معلقة",
                DueDate = "غداً",
                Priority = TaskPriority.High
            },
            new TaskItem
            {
                Title = "تحديث أسعار الأصناف",
                Description = "تحديث أسعار الأصناف حسب السوق",
                DueDate = "خلال أسبوع",
                Priority = TaskPriority.Medium
            },
            new TaskItem
            {
                Title = "إعداد تقرير شهري",
                Description = "إعداد التقرير المالي الشهري",
                DueDate = "نهاية الشهر",
                Priority = TaskPriority.Low
            }
        };

        PendingTasks.Clear();
        foreach (var task in tasksList)
        {
            PendingTasks.Add(task);
        }
    }

    private void UpdateWelcomeMessage()
    {
        var currentUser = _sessionService.CurrentUser;
        if (currentUser != null)
        {
            var timeOfDay = DateTime.Now.Hour switch
            {
                >= 5 and < 12 => "صباح الخير",
                >= 12 and < 17 => "مساء الخير",
                >= 17 and < 22 => "مساء الخير",
                _ => "ليلة سعيدة"
            };

            WelcomeMessage = $"{timeOfDay} {currentUser.FullName ?? currentUser.Name}";
        }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            // تحميل إحصائيات المبيعات (مثال - يحتاج تطبيق حسب الجداول الفعلية)
            TotalSales = await GetTotalSalesAsync();
            
            // تحميل إحصائيات المشتريات
            TotalPurchases = await GetTotalPurchasesAsync();
            
            // تحميل عدد العملاء
            TotalCustomers = await GetTotalCustomersAsync();
            
            // تحميل عدد الموردين
            TotalSuppliers = await GetTotalSuppliersAsync();
        }
        catch (Exception ex)
        {
            // معالجة الأخطاء
            System.Diagnostics.Debug.WriteLine($"خطأ في تحميل الإحصائيات: {ex.Message}");
        }
    }

    private async Task<decimal> GetTotalSalesAsync()
    {
        // مثال - يحتاج تطبيق حسب الجداول الفعلية
        // return await _context.SalesInvoices.SumAsync(x => x.TotalAmount);
        return 1250000.50m; // قيمة تجريبية
    }

    private async Task<decimal> GetTotalPurchasesAsync()
    {
        // مثال - يحتاج تطبيق حسب الجداول الفعلية
        // return await _context.PurchaseInvoices.SumAsync(x => x.TotalAmount);
        return 890000.75m; // قيمة تجريبية
    }

    private async Task<int> GetTotalCustomersAsync()
    {
        // مثال - يحتاج تطبيق حسب الجداول الفعلية
        // return await _context.Customers.CountAsync();
        return 156; // قيمة تجريبية
    }

    private async Task<int> GetTotalSuppliersAsync()
    {
        return await _context.Suppliers.CountAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        var notificationsList = new List<NotificationItem>
        {
            new NotificationItem
            {
                Title = "تنبيه مهم",
                Message = "يوجد 5 فواتير مبيعات معلقة تحتاج للمراجعة",
                Date = DateTime.Now.AddHours(-2).ToString("dd/MM/yyyy HH:mm"),
                Type = NotificationType.Warning
            },
            new NotificationItem
            {
                Title = "تذكير",
                Message = "موعد استحقاق فاتورة مورد رقم INV-2024-001 غداً",
                Date = DateTime.Now.AddHours(-1).ToString("dd/MM/yyyy HH:mm"),
                Type = NotificationType.Info
            },
            new NotificationItem
            {
                Title = "تحذير",
                Message = "مخزون الصنف ABC-123 منخفض (أقل من الحد الأدنى)",
                Date = DateTime.Now.AddMinutes(-30).ToString("dd/MM/yyyy HH:mm"),
                Type = NotificationType.Danger
            }
        };

        Notifications.Clear();
        foreach (var notification in notificationsList)
        {
            Notifications.Add(notification);
        }
    }

    private async Task LoadPendingTasksAsync()
    {
        var tasksList = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "مراجعة فواتير المبيعات",
                Description = "مراجعة 5 فواتير مبيعات معلقة",
                DueDate = "غداً",
                Priority = TaskPriority.High
            },
            new TaskItem
            {
                Title = "تحديث أسعار الأصناف",
                Description = "تحديث أسعار الأصناف حسب السوق",
                DueDate = "خلال أسبوع",
                Priority = TaskPriority.Medium
            },
            new TaskItem
            {
                Title = "إعداد تقرير شهري",
                Description = "إعداد التقرير المالي الشهري",
                DueDate = "نهاية الشهر",
                Priority = TaskPriority.Low
            }
        };

        PendingTasks.Clear();
        foreach (var task in tasksList)
        {
            PendingTasks.Add(task);
        }
    }

    #region Commands

    [RelayCommand]
    private async Task AddCustomerAsync()
    {
        // فتح شاشة إضافة عميل جديد
        // يمكن استخدام NavigationService أو EventAggregator
        System.Diagnostics.Debug.WriteLine("فتح شاشة إضافة عميل جديد");
    }

    [RelayCommand]
    private async Task AddSupplierAsync()
    {
        // فتح شاشة إضافة مورد جديد
        System.Diagnostics.Debug.WriteLine("فتح شاشة إضافة مورد جديد");
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        // فتح شاشة إضافة صنف جديد
        System.Diagnostics.Debug.WriteLine("فتح شاشة إضافة صنف جديد");
    }

    [RelayCommand]
    private async Task CreateSalesInvoiceAsync()
    {
        // فتح شاشة إنشاء فاتورة مبيعات
        System.Diagnostics.Debug.WriteLine("فتح شاشة إنشاء فاتورة مبيعات");
    }

    [RelayCommand]
    private async Task CreatePurchaseOrderAsync()
    {
        // فتح شاشة إنشاء أمر شراء
        System.Diagnostics.Debug.WriteLine("فتح شاشة إنشاء أمر شراء");
    }

    [RelayCommand]
    private async Task CreateReceiptVoucherAsync()
    {
        // فتح شاشة إنشاء سند قبض
        System.Diagnostics.Debug.WriteLine("فتح شاشة إنشاء سند قبض");
    }

    [RelayCommand]
    private async Task CreatePaymentVoucherAsync()
    {
        // فتح شاشة إنشاء سند دفع
        System.Diagnostics.Debug.WriteLine("فتح شاشة إنشاء سند دفع");
    }

    [RelayCommand]
    private async Task ViewFinancialReportAsync()
    {
        // فتح شاشة التقارير المالية
        System.Diagnostics.Debug.WriteLine("فتح شاشة التقارير المالية");
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await InitializeAsync();
    }

    #endregion
}

// نماذج البيانات المساعدة
public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
}

public class TaskItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
}

public enum NotificationType
{
    Info,
    Warning,
    Danger,
    Success
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}