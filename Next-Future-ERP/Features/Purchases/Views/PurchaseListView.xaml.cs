using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Purchases.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Next_Future_ERP.Features.Purchases.Views
{
    /// <summary>
    /// Interaction logic for PurchaseListView.xaml
    /// </summary>
    public partial class PurchaseListView : Page
    {
        public PurchaseListView(char txnType) // 'P' أو 'R'
        {
            InitializeComponent();
            var factory = App.ServiceProvider.GetRequiredService<Func<char, PurchaseListViewModel>>();
            DataContext = factory.Invoke(txnType);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PurchaseListViewModel vm
                && vm.EditDialogCommand.CanExecute(null))
            {
                vm.EditDialogCommand.Execute(null);
            }
        }

        private void TestItems_Click(object sender, RoutedEventArgs e)
        {
            var testWindow = new TestPurchaseOrderWindow();
            testWindow.Owner = Application.Current.MainWindow;
            testWindow.ShowDialog();
        }
    }
}
