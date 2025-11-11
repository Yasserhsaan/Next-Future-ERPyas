using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.SystemUsers.ViewModels;
using Next_Future_ERP.Features.SystemUsers.Views;

namespace Next_Future_ERP.Features.SystemUsers.Views
{
    /// <summary>
    /// Interaction logic for SystemUsersListView.xaml
    /// </summary>
    public partial class SystemUsersListView : Page
    {
        private SystemUsersListViewModel _viewModel;

        public SystemUsersListView()
        {
            InitializeComponent();
            Loaded += SystemUsersListView_Loaded;
        }

        private async void SystemUsersListView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel = App.ServiceProvider.GetRequiredService<SystemUsersListViewModel>();
                DataContext = _viewModel;
                await _viewModel.InitializeAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الصفحة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewModel?.SelectedUser != null)
            {
                _viewModel.EditUserCommand.Execute(_viewModel.SelectedUser);
            }
        }
    }
}
