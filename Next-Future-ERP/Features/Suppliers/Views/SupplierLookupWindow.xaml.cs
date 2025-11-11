using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Suppliers.Views
{
    public partial class SupplierLookupWindow : Window
    {
        public Supplier? SelectedSupplier { get; private set; }

        public SupplierLookupWindow()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<SupplierLookupViewModel>();
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SupplierLookupViewModel vm && vm.Selected != null)
            {
                SelectedSupplier = vm.Selected;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("اختر صفاً أولاً.", "تنبيه");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Choose_Click(sender, e);
        }
    }
}


