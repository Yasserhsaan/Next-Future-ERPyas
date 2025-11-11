using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Warehouses.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Warehouses.Views
{
    /// <summary>
    /// Interaction logic for ValuationGroupView.xaml
    /// </summary>
    public partial class ValuationGroupView : Page
    {
        public ValuationGroupView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<ValuationGroupViewModel>();
            
            // تأكد من أن CommandBar يحصل على DataContext
            Loaded += (s, e) =>
            {
                if (DataContext is ValuationGroupViewModel vm)
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
            if (DataContext is ValuationGroupViewModel vm
                && vm.EditDialogCommand.CanExecute(null))
            {
                vm.EditDialogCommand.Execute(null);
            }
        }
    }
}
