using Next_Future_ERP.Features.Accounts.ViewModels;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Views
{
    /// <summary>
    /// Interaction logic for FundEditWindow.xaml
    /// </summary>
    public partial class FundEditWindow : Window
    {
        public FundEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is FundEditViewModel vm)
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

