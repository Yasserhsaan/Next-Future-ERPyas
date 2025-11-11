using Next_Future_ERP.Features.Warehouses.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace Next_Future_ERP.Features.Warehouses.Views
{
    /// <summary>
    /// Interaction logic for ValuationGroupEditWindow.xaml
    /// </summary>
    public partial class ValuationGroupEditWindow : FluentWindow
    {
        public ValuationGroupEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ValuationGroupEditViewModel vm)
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
