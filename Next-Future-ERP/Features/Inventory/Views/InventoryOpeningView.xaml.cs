using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Inventory.ViewModels;
using Next_Future_ERP;
using Next_Future_ERP.Features.Inventory.Views;
using System.Windows.Controls;
using System.Windows;
using System;

namespace Next_Future_ERP.Features.Inventory.Views
{
    /// <summary>
    /// Interaction logic for InventoryOpeningView.xaml
    /// </summary>
    public partial class InventoryOpeningView : UserControl
    {
        public InventoryOpeningView()
        {
            InitializeComponent();
            // ربط الـ ViewModel تلقائياً عند إنشاء التحكم من XAML
            if (DataContext == null && App.ServiceProvider != null)
            {
                var vm = App.ServiceProvider.GetService<InventoryOpeningViewModel>();
                if (vm != null)
                    DataContext = vm;
            }
        }

        private async void BrowseDrafts_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var browseVm = App.ServiceProvider.GetService<InventoryOpeningBrowseViewModel>();
                var win = new InventoryOpeningBrowseView(browseVm!){ Owner = Application.Current.Windows[0] };
                if (win.ShowDialog() == true)
                {
                    var hdr = browseVm!.SelectedHeader;
                    if (hdr != null)
                    {
                        var vm = (InventoryOpeningViewModel)DataContext;
                        await vm.LoadDocumentAsync(hdr.DocID);
                        
                        // إشعار المستخدم بنجاح التحميل
                        MessageBox.Show($"تم تحميل المستند رقم: {hdr.DocNo}\nالتاريخ: {hdr.DocDate:yyyy/MM/dd}", 
                            "نجح التحميل", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المستند: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public InventoryOpeningView(InventoryOpeningViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
