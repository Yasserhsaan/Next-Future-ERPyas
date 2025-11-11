using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Dashboard.Models;
using Next_Future_ERP.Features.Dashboard.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Dashboard.ViewModels
{
    public partial class PurchaseDashboardViewModel : ObservableObject
    {
        private readonly IPurchaseDashboardService _dashboardService;

        [ObservableProperty] private PurchaseDashboardData dashboardData = new();
        [ObservableProperty] private ObservableCollection<RecentItem> recentItems = new();
        [ObservableProperty] private ObservableCollection<AlertItem> alerts = new();
        [ObservableProperty] private bool isLoading;

        public PurchaseDashboardViewModel(IPurchaseDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
            _ = LoadDashboardDataAsync();
        }

        [RelayCommand]
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;

                // تحميل بيانات الداش بورد
                DashboardData = await _dashboardService.GetDashboardDataAsync();

                // تحميل العناصر الحديثة
                var recent = await _dashboardService.GetRecentItemsAsync(10);
                RecentItems.Clear();
                foreach (var item in recent)
                    RecentItems.Add(item);

                // تحميل التنبيهات
                var alertList = await _dashboardService.GetAlertsAsync();
                Alerts.Clear();
                foreach (var alert in alertList)
                    Alerts.Add(alert);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل بيانات الداش بورد: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // إعداد بيانات افتراضية في حالة الخطأ
                DashboardData = new PurchaseDashboardData();
                RecentItems.Clear();
                Alerts.Clear();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void RefreshDashboard()
        {
            _ = LoadDashboardDataAsync();
        }

        [RelayCommand]
        private void NavigateToPurchaseOrders()
        {
            // TODO: التنقل إلى شاشة أوامر الشراء
            MessageBox.Show("التنقل إلى أوامر الشراء", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void NavigateToStoreReceipts()
        {
            // TODO: التنقل إلى شاشة سندات الاستلام
            MessageBox.Show("التنقل إلى سندات الاستلام", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void NavigateToPurchaseInvoices()
        {
            // TODO: التنقل إلى شاشة فواتير المشتريات
            MessageBox.Show("التنقل إلى فواتير المشتريات", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void NavigateToSuppliers()
        {
            // TODO: التنقل إلى شاشة الموردين
            MessageBox.Show("التنقل إلى الموردين", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void HandleAlertAction(AlertItem alert)
        {
            switch (alert.ActionCommand)
            {
                case "ViewPendingPOs":
                    NavigateToPurchaseOrders();
                    break;
                case "ViewPendingReceipts":
                    NavigateToStoreReceipts();
                    break;
                case "ViewDraftInvoices":
                    NavigateToPurchaseInvoices();
                    break;
                default:
                    MessageBox.Show($"تنفيذ الإجراء: {alert.ActionText}", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }

            // تمييز التنبيه كمقروء
            alert.IsRead = true;
        }

        [RelayCommand]
        private void ViewRecentItem(RecentItem item)
        {
            switch (item.Type)
            {
                case "PO":
                    NavigateToPurchaseOrders();
                    break;
                case "GRN":
                    NavigateToStoreReceipts();
                    break;
                case "PI":
                case "PR":
                    NavigateToPurchaseInvoices();
                    break;
                default:
                    MessageBox.Show($"عرض العنصر: {item.Title}", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }
    }
}
