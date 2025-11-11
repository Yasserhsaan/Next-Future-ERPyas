using System.Windows;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.StoreReceipts.Views
{
    public partial class StoreReceiptEditWindow : Window
    {
        public StoreReceiptEditWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // التحقق من الكمية إذا كانت الخلية المحدثة هي الكمية
            if (e.Column.Header.ToString() == "الكمية" && e.Row.Item is StoreReceipts.Models.StoreReceiptDetailed detail)
            {
                if (detail.Quantity > detail.RemainingQuantity)
                {
                    // إرجاع القيمة إلى الكمية المتبقية
                    detail.Quantity = detail.RemainingQuantity;
                    
                    MessageBox.Show($"الكمية المدخلة تتجاوز الكمية المتبقية من أمر الشراء ({detail.RemainingQuantity}). تم ضبط الكمية على الحد الأقصى المسموح.", 
                        "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // تحديث الحسابات عند انتهاء تعديل الخلية
            if (DataContext is ViewModels.StoreReceiptEditViewModel viewModel)
            {
                viewModel.OnDetailPropertyChanged();
            }
        }
    }
}
