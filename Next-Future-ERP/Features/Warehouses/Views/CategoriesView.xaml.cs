using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.Warehouses.Views
{
    public partial class CategoriesView : Page
    {
        public CategoriesView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<CategoriesViewModel>();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is CategoriesViewModel.CategoryViewModelItem treeItem && DataContext is CategoriesViewModel viewModel)
            {
                viewModel.SelectCategoryFromTree(treeItem);
            }
        }
    }
} 