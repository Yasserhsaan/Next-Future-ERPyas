using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Dashboard.ViewModels;

namespace Next_Future_ERP.Features.Dashboard.Views
{
    public partial class PurchaseDashboardView : Page
    {
        public PurchaseDashboardView()
        {
            InitializeComponent();
            
            // ربط ViewModel مع View
            try
            {
                if (App.ServiceProvider != null)
                {
                    DataContext = App.ServiceProvider.GetRequiredService<PurchaseDashboardViewModel>();
                }
                else
                {
                    // إنشاء ViewModel مباشرة إذا لم يكن ServiceProvider متاحاً
                    DataContext = new PurchaseDashboardViewModel(
                        App.ServiceProvider?.GetRequiredService<Next_Future_ERP.Features.Dashboard.Services.IPurchaseDashboardService>() 
                        ?? new Next_Future_ERP.Features.Dashboard.Services.PurchaseDashboardService(
                            App.ServiceProvider?.GetRequiredService<Next_Future_ERP.Data.AppDbContext>() 
                            ?? throw new InvalidOperationException("AppDbContext غير متاح")));
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"خطأ في تهيئة الداش بورد: {ex.Message}", "خطأ", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
