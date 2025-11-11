using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Warehouses.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Warehouses.Views
{
    /// <summary>
    /// Interaction logic for WarehousesView.xaml
    /// </summary>
    public partial class WarehousesView : Page
    {
        public WarehousesView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<WarehousesViewModel>();
            
            // تأكد من أن CommandBar يحصل على DataContext
            Loaded += (s, e) =>
            {
                if (DataContext is WarehousesViewModel vm)
                {
                    // تحديث CanExecute للأوامر
                    if (vm.EditDialogCommand is RelayCommand editCmd)
                        editCmd.NotifyCanExecuteChanged();
                    if (vm.DeleteCommand is AsyncRelayCommand deleteCmd)
                        deleteCmd.NotifyCanExecuteChanged();
                }
            };
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is WarehousesViewModel vm
                && vm.EditDialogCommand.CanExecute(null))
            {
                vm.EditDialogCommand.Execute(null);
            }
        }
    }
}
