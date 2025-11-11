using Next_Future_ERP.Features.Warehouses.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace Next_Future_ERP.Features.Warehouses.Views
{
    /// <summary>
    /// Interaction logic for WarehouseEditWindow.xaml
    /// </summary>
    public partial class WarehouseEditWindow : FluentWindow
    {
        public WarehouseEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is WarehouseEditViewModel vm)
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
