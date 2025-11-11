using System.Windows.Controls;
using Next_Future_ERP.Features.Items.ViewModels;
using System.Windows;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Views
{
    public partial class ItemComponentsView : UserControl
    {
        public ItemComponentsView()
        {
            InitializeComponent();
            Loaded += ItemComponentsView_Loaded;
        }

        private async void ItemComponentsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ItemComponentsViewModel vm)
            {
                await vm.LoadAsync();
            }
        }
    }
}
