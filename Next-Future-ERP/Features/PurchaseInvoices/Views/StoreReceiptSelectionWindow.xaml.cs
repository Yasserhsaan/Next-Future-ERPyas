using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.StoreReceipts.Models;
using Next_Future_ERP.Features.StoreReceipts.Services;

namespace Next_Future_ERP.Features.PurchaseInvoices.Views
{
    public partial class StoreReceiptSelectionWindow : Window
    {
        private readonly IStoreReceiptsService _storeReceiptsService;
        private ObservableCollection<StoreReceipt> _allStoreReceipts = new();
        private ObservableCollection<StoreReceipt> _filteredStoreReceipts = new();

        public StoreReceipt? SelectedStoreReceipt { get; private set; }

        public StoreReceiptSelectionWindow()
        {
            InitializeComponent();
            _storeReceiptsService = App.ServiceProvider.GetRequiredService<IStoreReceiptsService>();
            
            // ضبط DataContext للـ DataGrid
            StoreReceiptsGrid.DataContext = _filteredStoreReceipts;
            
            System.Diagnostics.Debug.WriteLine("StoreReceiptSelectionWindow opened - loading receipts...");
            _ = LoadStoreReceiptsAsync();
        }

        private async Task LoadStoreReceiptsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadStoreReceiptsAsync - calling GetAllAsync()...");
                var storeReceipts = await _storeReceiptsService.GetAllAsync();
                System.Diagnostics.Debug.WriteLine($"LoadStoreReceiptsAsync - received {storeReceipts.Count} receipts from service");
                
                // تصفية سندات الاستلام المعتمدة فقط (Status = 2)
                var approvedReceipts = storeReceipts.Where(sr => sr.Status == 2).ToList();
                
                // تصحيح لرؤية السندات المحملة
                System.Diagnostics.Debug.WriteLine($"Total receipts loaded: {storeReceipts.Count}");
                System.Diagnostics.Debug.WriteLine($"Approved receipts (Status 2): {approvedReceipts.Count}");
                foreach (var receipt in storeReceipts)
                {
                    System.Diagnostics.Debug.WriteLine($"All Receipt: {receipt.ReceiptNumber}, Status: {receipt.Status}");
                }
                foreach (var receipt in approvedReceipts)
                {
                    System.Diagnostics.Debug.WriteLine($"Approved Receipt: {receipt.ReceiptNumber}, Status: {receipt.Status}");
                }
                
                _allStoreReceipts.Clear();
                _filteredStoreReceipts.Clear();
                
                System.Diagnostics.Debug.WriteLine($"Adding {approvedReceipts.Count} receipts to collections...");
                foreach (var receipt in approvedReceipts.OrderByDescending(x => x.ReceiptDate))
                {
                    _allStoreReceipts.Add(receipt);
                    _filteredStoreReceipts.Add(receipt);
                    System.Diagnostics.Debug.WriteLine($"Added to collections: {receipt.ReceiptNumber}");
                }
                
                System.Diagnostics.Debug.WriteLine($"Final collections count - _allStoreReceipts: {_allStoreReceipts.Count}, _filteredStoreReceipts: {_filteredStoreReceipts.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل سندات الاستلام: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            
            _filteredStoreReceipts.Clear();
            
            var filtered = _allStoreReceipts.Where(sr => 
                sr.ReceiptNumber.ToLower().Contains(searchText) ||
                (sr.SupplierName?.ToLower().Contains(searchText) ?? false) ||
                (sr.Description?.ToLower().Contains(searchText) ?? false)
            ).ToList();
            
            foreach (var receipt in filtered)
            {
                _filteredStoreReceipts.Add(receipt);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Refresh button clicked - reloading store receipts");
            _ = LoadStoreReceiptsAsync();
        }

        private void StoreReceiptsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedStoreReceipt = StoreReceiptsGrid.SelectedItem as StoreReceipt;
            SelectButton.IsEnabled = SelectedStoreReceipt != null;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedStoreReceipt != null)
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
