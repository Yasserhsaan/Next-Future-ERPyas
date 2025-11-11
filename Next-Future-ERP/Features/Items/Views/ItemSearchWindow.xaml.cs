using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Items.ViewModels;
using System.Windows;

namespace Next_Future_ERP.Features.Items.Views
{
    /// <summary>
    /// Interaction logic for ItemSearchWindow.xaml
    /// </summary>
    public partial class ItemSearchWindow : Window
    {
        public Item? SelectedItem { get; private set; }

        public ItemSearchWindow(IItemsService itemsService)
        {
            InitializeComponent();
            DataContext = new ItemSearchWindowViewModel(itemsService);
            
            // تحميل الأصناف تلقائياً عند فتح النافذة
            Loaded += async (s, e) =>
            {
                if (DataContext is ItemSearchWindowViewModel vm)
                {
                    await vm.LoadItemsCommand.ExecuteAsync(null);
                }
            };
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is ItemSearchWindowViewModel vm && vm.SelectedItem is not null)
            {
                DialogResult = true;
                SelectedItem = vm.SelectedItem;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}