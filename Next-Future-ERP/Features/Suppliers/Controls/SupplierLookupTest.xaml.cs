using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Suppliers.Views;
using System.Windows;

namespace Next_Future_ERP.Features.Suppliers.Controls
{
    public partial class SupplierLookupTest : Window
    {
        public SupplierLookupTest()
        {
            InitializeComponent();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var searchWindow = App.ServiceProvider.GetRequiredService<SupplierLookupWindow>();
            searchWindow.Owner = this;
            
            if (searchWindow.ShowDialog() == true && searchWindow.SelectedSupplier is not null)
            {
                MessageBox.Show($"تم اختيار المورد: {searchWindow.SelectedSupplier.SupplierName}", 
                               "نجح الاختيار", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
