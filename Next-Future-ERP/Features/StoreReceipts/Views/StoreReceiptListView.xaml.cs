using Next_Future_ERP.Features.StoreReceipts.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.StoreReceipts.Views
{
    public partial class StoreReceiptListView : Page
    {
        public StoreReceiptListView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<StoreReceiptListViewModel>();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is StoreReceiptListViewModel viewModel)
            {
                viewModel.EditDialogCommand.Execute(null);
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is StoreReceipts.Models.StoreReceipt receipt)
            {
                System.Diagnostics.Debug.WriteLine($"Loading row for: {receipt.ReceiptNumber}, Status: {receipt.Status}, StatusText: {receipt.StatusText}");
            }
        }
    }
}
