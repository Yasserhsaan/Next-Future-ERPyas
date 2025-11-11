using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Items.Views
{
    public partial class ItemLookupWindow : Window
    {
        public Item? SelectedItem { get; private set; }

        public ItemLookupWindow(ItemLookupViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ItemLookupViewModel vm && vm.Selected != null)
            {
                SelectedItem = vm.Selected;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("اختر صفاً أولاً.", "تنبيه");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Choose_Click(sender, e);
        }
    }
}
