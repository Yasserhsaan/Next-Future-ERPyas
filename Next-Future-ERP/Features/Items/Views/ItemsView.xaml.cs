using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Items.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Items.Views
{
    /// <summary>
    /// Interaction logic for ItemsView.xaml
    /// </summary>
    public partial class ItemsView : Page
    {
        public ItemsView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<ItemsViewModel>();
            
            // تأكد من أن CommandBar يحصل على DataContext
            Loaded += (s, e) =>
            {
                if (DataContext is ItemsViewModel vm)
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
            if (DataContext is ItemsViewModel vm
                && vm.EditDialogCommand.CanExecute(null))
            {
                vm.EditDialogCommand.Execute(null);
            }
        }
    }
}
