using Next_Future_ERP.Features.Accounts.ViewModels;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Views
{
    /// <summary>
    /// Interaction logic for ReceiptVoucherEditWindow.xaml
    /// </summary>
    public partial class ReceiptVoucherEditWindow : Window
    {
        public ReceiptVoucherEditWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReceiptVoucherEditViewModel vm)
            {
                vm.CloseRequested += (sender, result) =>
                {
                    DialogResult = result;
                    Close();
                };

                // تحميل الخيارات
                await vm.LoadOptionsAsync();
            }
        }
    }
}

