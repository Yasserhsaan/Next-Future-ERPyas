using Next_Future_ERP.Features.Accounts.ViewModels;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Views
{
    /// <summary>
    /// Interaction logic for DebitCreditNotificationEditWindow.xaml
    /// </summary>
    public partial class DebitCreditNotificationEditWindow : Window
    {
        public DebitCreditNotificationEditWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DebitCreditNotificationEditViewModel vm)
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

