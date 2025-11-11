using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Accounts.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Accounts.Views
{
    /// <summary>
    /// Interaction logic for NextCurrenciesView.xaml
    /// </summary>
    public partial class NextCurrenciesView : Page
    {
        public NextCurrenciesView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<NextCurrenciesViewModel>();
            
            // تأكد من أن CommandBar يحصل على DataContext
            Loaded += (s, e) =>
            {
                if (DataContext is NextCurrenciesViewModel vm)
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
            if (DataContext is NextCurrenciesViewModel vm
                && vm.EditDialogCommand.CanExecute(null))
            {
                vm.EditDialogCommand.Execute(null);
            }
        }
    }
}
