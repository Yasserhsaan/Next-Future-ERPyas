using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.PrintManagement.Models; 
using Next_Future_ERP.Features.PrintManagement.Services;
using Next_Future_ERP.Features.PrintManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.PrintManagement.Views
{
    public partial class TemplateLibraryView : Page
    {
        private readonly TemplateLibraryViewModel _vm;
        private readonly PrintManagementInitializationService _initService;
        private readonly PrintManagementSeedDataService _seedService;

        // مخصص للـNavigation، نقرأ كل الخدمات من DI
        public TemplateLibraryView()
        {
            InitializeComponent();

            _vm = App.ServiceProvider.GetRequiredService<TemplateLibraryViewModel>();
            _initService = App.ServiceProvider.GetRequiredService<PrintManagementInitializationService>();
            _seedService = App.ServiceProvider.GetRequiredService<PrintManagementSeedDataService>();

            DataContext = _vm;
            Loaded += async (_, __) =>
            {
                try { await _vm.LoadDataCommand.ExecuteAsync(null); }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"❌ خطأ في تحميل البيانات:\n{ex.Message}", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        // زر تهيئة النظام
        private async void InitializeSystemButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("🔄 جاري تهيئة النظام وإضافة البيانات التجريبية...",
                    "تهيئة", MessageBoxButton.OK, MessageBoxImage.Information);

                await _seedService.SeedPrintManagementDataAsync();
                await _vm.LoadDataCommand.ExecuteAsync(null);

                MessageBox.Show("✅ تم تهيئة النظام بنجاح!\nتم إضافة قوالب تجريبية جاهزة.",
                    "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ خطأ في التهيئة:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // زر فحص النظام
        private async void CheckSystemButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var status = await _initService.CheckSystemStatusAsync();
                var message =
                    $"📊 تقرير حالة النظام:\n\n" +
                    $"🔗 قاعدة البيانات: {(status.DatabaseConnected ? "✅ متصلة" : "❌ غير متصلة")}\n" +
                    $"🗃️ الجداول: {(status.TablesExist ? "✅ موجودة" : "❌ غير موجودة")}\n" +
                    $"📄 البيانات: {(status.HasSampleData ? "✅ موجودة" : "❌ غير موجودة")}\n\n" +
                    $"• القوالب: {status.TemplatesCount}\n" +
                    $"• الإصدارات: {status.VersionsCount}\n" +
                    $"• المحتويات: {status.ContentsCount}\n" +
                    $"• الأصول: {status.AssetsCount}\n" +
                    $"• المهام: {status.JobsCount}";

                MessageBox.Show(message, "حالة النظام", MessageBoxButton.OK,
                    status.IsReady ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ خطأ في الفحص:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void CreateNewTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is not TemplateLibraryViewModel vm)
                {
                    MessageBox.Show("تعذر الوصول إلى البيانات.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // تحقق من المدخلات الأساسية
                if (vm.SelectedCompanyId is null || vm.SelectedCompanyId <= 0)
                {
                    MessageBox.Show("يرجى اختيار الشركة أولاً.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (vm.SelectedDocumentTypeId is null || vm.SelectedDocumentTypeId <= 0)
                {
                    MessageBox.Show("يرجى اختيار نوع المستند أولاً.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // جهّز TemplateInfo بقيم فعلية
                var info = new Next_Future_ERP.Features.PrintManagement.Models.TemplateInfo
                {
                    Name = "قالب جديد",
                    CompanyId = vm.SelectedCompanyId,
                    BranchId = vm.SelectedBranchId,         // ممكن تكون null
                    DocumentTypeId = vm.SelectedDocumentTypeId,
                    CompanyName = vm.CompanyOptions.FirstOrDefault(c => c.CompId == vm.SelectedCompanyId)?.CompName ?? "",
                    BranchName = vm.BranchOptions.FirstOrDefault(b => b.BranchId == vm.SelectedBranchId)?.BranchName,
                    DocumentTypeName = vm.DocumentTypeOptions.FirstOrDefault(d => d.Key == vm.SelectedDocumentTypeId).Value,
                    Locale = vm.SelectedLocale ?? "ar-SA",
                    Engine = "html", // مهم: متوافق مع CHECK CONSTRAINT في الجدول
                    Active = true,
                    IsDefault = false,
                    ActiveVersionNo = 1,
                    Status = "جديد"
                };

                // احصل على مساحة العمل وافتح النافذة
                var workspaceView = App.ServiceProvider.GetRequiredService<Next_Future_ERP.Features.PrintManagement.Views.TemplateWorkspaceView>();
                workspaceView.CreateNewTemplate(info);

                var wnd = new Window
                {
                    Title = $"مساحة العمل: {info.Name}",
                    Content = workspaceView,
                    Width = 1400,
                    Height = 900,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowState = WindowState.Maximized
                };
                wnd.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // زر فتح مساحة العمل (تحذير فقط إن كان النظام غير جاهز)
        private async void OpenWorkspaceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is not TemplateLibraryViewModel vm)
                {
                    MessageBox.Show("تعذر الوصول إلى البيانات.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (vm.SelectedTemplate == null)
                {
                    MessageBox.Show("يرجى اختيار قالب من القائمة أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // احصل على مساحة العمل من الـ DI
                var workspaceView = App.ServiceProvider.GetRequiredService<Next_Future_ERP.Features.PrintManagement.Views.TemplateWorkspaceView>();

                // حمّل القالب المحدد
                workspaceView.LoadTemplate(vm.SelectedTemplate.TemplateId);

                // افتح النافذة
                var wnd = new Window
                {
                    Title = $"مساحة العمل: {vm.SelectedTemplate.Name}",
                    Content = workspaceView,
                    Width = 1400,
                    Height = 900,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowState = WindowState.Maximized
                };
                wnd.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
