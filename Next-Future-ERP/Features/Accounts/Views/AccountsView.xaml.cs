using Microsoft.Data.SqlClient;
using Next_Future_ERP.Core.Services.Contracts;
using Next_Future_ERP.Features.Accounts.ViewModels;
using Next_Future_ERP.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
               // await Task.Delay(100); // فقط للسماح بتحميل UI
                await vm.ReloadTreeAsync(); // تحميل التابات والشجرة
            }
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is AccountsViewModel vm)
            {
                vm.SelectedAccount = e.NewValue as Account;
            }
        }
        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button closeButton)
            {
                if (closeButton.DataContext is Account accountToClose)
                {
                    var viewModel = DataContext as AccountsViewModel;
                    if (viewModel != null)
                    {
                        viewModel.RootAccounts.Remove(accountToClose);

                        // إعادة تعيين التاب المحدد إذا تم إغلاق التاب النشط
                        if (viewModel.SelectedTabAccount == accountToClose)
                        {
                            viewModel.SelectedTabAccount = viewModel.RootAccounts.FirstOrDefault();
                        }
                    }
                }
            }
        }

    }
}
