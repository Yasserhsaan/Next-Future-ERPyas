using Next_Future_ERP.Features.Accounts.ViewModels;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Views
{
    /// <summary>
    /// Interaction logic for BankEditWindow.xaml
    /// </summary>
    public partial class BankEditWindow : Window
    {
        public BankEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is BankEditViewModel vm)
            {
                vm.CloseRequested += (sender, result) =>
                {
                    DialogResult = result;
                    Close();
                };
            }
        }
    }
}

