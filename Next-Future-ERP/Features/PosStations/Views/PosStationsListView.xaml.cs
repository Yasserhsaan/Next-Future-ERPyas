using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.PosStations.ViewModels;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.PosStations.Views
{
    /// <summary>
    /// Interaction logic for PosStationsListView.xaml
    /// </summary>
    public partial class PosStationsListView : Page
    {
        private PosStationsListViewModel _viewModel;

        public PosStationsListView(PosStationsListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += OnLoaded;
        }

        // Constructor بدون parameters للاستخدام مع Activator.CreateInstance
        public PosStationsListView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                await _viewModel.InitializeAsync();
            }
            else
            {
                // إنشاء ViewModel باستخدام DI
                var serviceProvider = App.ServiceProvider;
                if (serviceProvider != null)
                {
                    _viewModel = serviceProvider.GetRequiredService<PosStationsListViewModel>();
                    DataContext = _viewModel;
                    await _viewModel.InitializeAsync();
                }
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null && _viewModel != null)
            {
                _viewModel.EditStationCommand.Execute(dataGrid.SelectedItem);
            }
        }
    }
}
