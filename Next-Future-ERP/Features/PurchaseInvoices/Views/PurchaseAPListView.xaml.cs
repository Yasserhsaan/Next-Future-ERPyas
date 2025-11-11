using Next_Future_ERP.Features.PurchaseInvoices.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.PurchaseInvoices.Views
{
    public partial class PurchaseAPListView : Page
    {
        public PurchaseAPListView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<PurchaseAPListViewModel>();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is PurchaseAPListViewModel viewModel)
            {
                viewModel.EditDialogCommand.Execute(null);
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // يمكن إضافة منطق إضافي هنا إذا لزم الأمر
        }

        private void DocTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (DataContext is PurchaseAPListViewModel viewModel)
                {
                    viewModel.SelectedDocType = selectedItem.Content.ToString() ?? "فاتورة مشتريات";
                }
            }
        }
    }
}
