using Next_Future_ERP.Features.Accounts.ViewModels;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Views
{
    /// <summary>
    /// Interaction logic for PaymentVoucherEditWindow.xaml
    /// </summary>
    public partial class PaymentVoucherEditWindow : Window
    {
        public PaymentVoucherEditWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaymentVoucherEditViewModel vm)
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

