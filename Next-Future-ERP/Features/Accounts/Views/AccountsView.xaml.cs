using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Auth.Services;
using Next_Future_ERP.Features.Accounts.ViewModels;
using Next_Future_ERP.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.Accounts.Views
{
    public partial class AccountsView : Page
    {
        public AccountsView()
        {
            InitializeComponent();
            Loaded += AccountsView_Loaded;
        }

        private async void AccountsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountsViewModel vm)
            {
                // خذ الـ Session من DI
                var session = App.ServiceProvider.GetRequiredService<ISessionService>();

                // ✅ جهّز CompanyId/BranchId في الـ VM (ويُسمح للـ XAML بالإنشاء)
                vm.InitializeFromSession(session);

                await vm.ReloadTreeAsync(); // سيضمن EnsureMainAccountsSeededAsync داخليًا
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is AccountsViewModel vm)
                vm.SelectedAccount = e.NewValue as Account;
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button closeButton && closeButton.DataContext is Account accountToClose)
            {
                if (DataContext is AccountsViewModel vm)
                {
                    vm.RootAccounts.Remove(accountToClose);
                    if (vm.SelectedTabAccount == accountToClose)
                        vm.SelectedTabAccount = vm.RootAccounts.FirstOrDefault();
                }
            }
        }
    }
}
