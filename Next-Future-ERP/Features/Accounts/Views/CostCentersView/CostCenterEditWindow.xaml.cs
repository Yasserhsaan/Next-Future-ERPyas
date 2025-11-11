using Next_Future_ERP.Features.Accounts.ViewModels;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Views
{
    /// <summary>
    /// Interaction logic for CostCenterEditWindow.xaml
    /// </summary>
    public partial class CostCenterEditWindow : Window
    {
        public CostCenterEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CostCenterEditViewModel vm)
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
