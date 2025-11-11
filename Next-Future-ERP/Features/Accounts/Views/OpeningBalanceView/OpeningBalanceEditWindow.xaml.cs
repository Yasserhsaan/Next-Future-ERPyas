using Next_Future_ERP.Features.Accounts.Controls;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.Accounts.ViewModels;
using Next_Future_ERP.Models;
using System.Windows;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.Accounts.Views
{
    /// <summary>
    /// Interaction logic for OpeningBalanceEditWindow.xaml
    /// </summary>
    public partial class OpeningBalanceEditWindow : Window
    {
        public OpeningBalanceEditWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is OpeningBalanceEditViewModel vm)
            {
                vm.CloseRequested += (sender, result) =>
                {
                    DialogResult = result;
                    Close();
                };

                // تحميل الخيارات
                await vm.LoadOptionsAsync();
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Set row header to row number
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void AccountSearchBox_AccountSelected(object sender, Account account)
        {
            // تحديث بيانات السطر الحالي في DataGrid
            if (DataContext is OpeningBalanceEditViewModel viewModel && 
                sender is AccountSearchBox searchBox)
            {
                // العثور على السطر الحالي في DataGrid
                var dataGrid = FindName("LinesDataGrid") as DataGrid;
                if (dataGrid?.SelectedItem is OpeningBalanceLine currentLine)
                {
                    currentLine.AccountId = account.AccountId;
                    currentLine.AccountCode = account.AccountCode;
                    currentLine.AccountNameAr = account.AccountNameAr;
                    currentLine.UsesCostCenter = account.UsesCostCenter ?? false;
                    
                    // إذا كان الحساب لا يتطلب مركز تكلفة، مسح مركز التكلفة
                    if (!currentLine.UsesCostCenter)
                    {
                        currentLine.CostCenterId = null;
                    }
                }
            }
        }
    }
}

