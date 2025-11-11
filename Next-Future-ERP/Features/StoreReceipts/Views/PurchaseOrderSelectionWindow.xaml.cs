using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Purchases.Models;
using Next_Future_ERP.Features.Purchases.Services;

namespace Next_Future_ERP.Features.StoreReceipts.Views
{
    public partial class PurchaseOrderSelectionWindow : Window
    {
        private readonly IPurchaseTxnsService _purchaseTxnsService;
        private ObservableCollection<PurchaseTxn> _allPurchaseOrders = new();
        private ObservableCollection<PurchaseTxn> _filteredPurchaseOrders = new();

        public PurchaseTxn? SelectedPurchaseOrder { get; private set; }

        public PurchaseOrderSelectionWindow()
        {
            InitializeComponent();
            _purchaseTxnsService = App.ServiceProvider.GetRequiredService<IPurchaseTxnsService>();
            PurchaseOrdersGrid.ItemsSource = _filteredPurchaseOrders;
            _ = LoadPurchaseOrdersAsync();
        }

        private async Task LoadPurchaseOrdersAsync()
        {
            try
            {
                var purchaseOrders = await _purchaseTxnsService.GetAllAsync('P'); // 'P' لأوامر الشراء
                
                // تصفية أوامر الشراء المعتمدة فقط (Status = 2)
                var approvedOrders = purchaseOrders.Where(po => po.Status == 2).ToList();
                
                _allPurchaseOrders.Clear();
                _filteredPurchaseOrders.Clear();
                
                foreach (var order in approvedOrders.OrderByDescending(x => x.TxnDate))
                {
                    _allPurchaseOrders.Add(order);
                    _filteredPurchaseOrders.Add(order);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل أوامر الشراء: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            
            _filteredPurchaseOrders.Clear();
            
            var filtered = _allPurchaseOrders.Where(po => 
                po.TxnNumber.ToLower().Contains(searchText) ||
                (po.SupplierName?.ToLower().Contains(searchText) ?? false) ||
                (po.Description?.ToLower().Contains(searchText) ?? false)
            ).ToList();
            
            foreach (var order in filtered)
            {
                _filteredPurchaseOrders.Add(order);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadPurchaseOrdersAsync();
        }

        private void PurchaseOrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedPurchaseOrder = PurchaseOrdersGrid.SelectedItem as PurchaseTxn;
            SelectButton.IsEnabled = SelectedPurchaseOrder != null;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPurchaseOrder != null)
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
