using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Purchases.Models;
using Next_Future_ERP.Features.Purchases.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.Purchases.Views
{
    /// <summary>
    /// Interaction logic for TestPurchaseOrderWindow.xaml
    /// </summary>
    public partial class TestPurchaseOrderWindow : Window
    {
        public TestPurchaseOrderWindow()
        {
            InitializeComponent();
            DataContext = new TestPurchaseOrderViewModel();
        }

        private void ItemSearchControl_ItemSelected(object sender, Item item)
        {
            if (DataContext is TestPurchaseOrderViewModel vm)
            {
                vm.OnItemSelected(sender, item);
            }
        }

        private void ItemSearchControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Next_Future_ERP.Features.Items.Controls.ItemSearchBox itemSearchBox)
            {
                itemSearchBox.LoadItems();
            }
        }

        // معالجات ItemSearchBox في DataGrid
        private void ItemSearchBox_ItemSelected(object sender, Item item)
        {
            if (DataContext is TestPurchaseOrderViewModel vm)
            {
                // الحصول على DataContext للـ ItemSearchBox (وهو PurchaseTxnDetail)
                if (sender is Next_Future_ERP.Features.Items.Controls.ItemSearchBox itemSearchBox)
                {
                    var dataContext = itemSearchBox.DataContext as PurchaseTxnDetail;
                    if (dataContext != null)
                    {
                        // تحديث الصنف المحدد
                        dataContext.ItemID = item.ItemID;
                        dataContext.UnitPrice = item.LastPurchasePrice ?? 0;
                        
                        // إعادة حساب الإجماليات
                        vm.RecalcTotalsCommand.Execute(null);
                    }
                }
            }
        }

        private void ItemSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Next_Future_ERP.Features.Items.Controls.ItemSearchBox itemSearchBox)
            {
                itemSearchBox.LoadItems();
            }
        }
    }
}
