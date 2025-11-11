using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Dashboard.ViewModels;
using Next_Future_ERP.Features.Dashboard.Views;
using System.Windows;

namespace Next_Future_ERP.Features.Dashboard
{
    public static class DashboardLauncher
    {
        public static void ShowPurchaseDashboard()
        {
            var viewModel = App.ServiceProvider.GetRequiredService<PurchaseDashboardViewModel>();
            var view = new PurchaseDashboardView { DataContext = viewModel };
            
            var window = new Window
            {
                Title = "داش بورد المشتريات",
                Content = view,
                Width = 1200,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResizeWithGrip
            };
            
            window.Show();
        }
    }
}
