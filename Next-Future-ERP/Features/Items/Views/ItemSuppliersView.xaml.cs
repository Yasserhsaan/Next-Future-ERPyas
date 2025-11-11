using System.Windows.Controls;
using System.Windows;

namespace Next_Future_ERP.Features.Items.Views
{
    public partial class ItemSuppliersView : UserControl
    {
        public ItemSuppliersView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is Next_Future_ERP.Features.Items.ViewModels.ItemSuppliersViewModel vm)
            {
                await vm.LoadAsync();
            }
        }
    }
}


