using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.PurchaseInvoices.ViewModels;

namespace Next_Future_ERP.Features.PurchaseInvoices.Views
{
    public partial class PurchaseAPEditWindow : Window
    {
        public PurchaseAPEditWindow()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("PurchaseAPEditWindow opened");
            
            // تصحيح إضافي لرؤية DataGrid
            if (DetailsGrid != null)
            {
                System.Diagnostics.Debug.WriteLine("DetailsGrid found in constructor");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DetailsGrid is null in constructor");
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (DataContext is ViewModels.PurchaseAPEditViewModel viewModel)
            {
                viewModel.OnDetailPropertyChanged();
            }
        }

        // طريقة لتحديث DataGrid يدوياً
        public void RefreshDataGrid()
        {
            System.Diagnostics.Debug.WriteLine("RefreshDataGrid called");
            if (DataContext is ViewModels.PurchaseAPEditViewModel viewModel)
            {
                DetailsGrid.ItemsSource = null;
                DetailsGrid.ItemsSource = viewModel.Details;
                System.Diagnostics.Debug.WriteLine($"DataGrid refreshed: {viewModel.Details.Count} items");
            }
        }

        private void DetailsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DetailsGrid loaded");
            if (DataContext is ViewModels.PurchaseAPEditViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"DetailsGrid DataContext: {viewModel.Details.Count} items");
                
                // تحديث DataGrid يدوياً
                DetailsGrid.ItemsSource = null;
                DetailsGrid.ItemsSource = viewModel.Details;
                System.Diagnostics.Debug.WriteLine($"DetailsGrid ItemsSource updated: {viewModel.Details.Count} items");
                
                // تصحيح إضافي لرؤية البيانات
                if (viewModel.Details.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"First detail: LineNo={viewModel.Details[0].LineNo}, ItemId={viewModel.Details[0].ItemId}, Quantity={viewModel.Details[0].Quantity}");
                }
                
                // تصحيح إضافي لرؤية DataGrid
                System.Diagnostics.Debug.WriteLine($"DataGrid ItemsSource: {DetailsGrid.ItemsSource}");
                System.Diagnostics.Debug.WriteLine($"DataGrid Items.Count: {DetailsGrid.Items.Count}");
                
                // تصحيح إضافي لرؤية DataGrid
                if (DetailsGrid.Items.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"DataGrid first item: {DetailsGrid.Items[0]}");
                }
            }
        }
    }
}
