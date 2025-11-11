using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.PosOperators.ViewModels;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.PosOperators.Views
{
    /// <summary>
    /// Interaction logic for PosOperatorsListView.xaml
    /// </summary>
    public partial class PosOperatorsListView : Page
    {
        private PosOperatorsListViewModel _viewModel;

        public PosOperatorsListView(PosOperatorsListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += OnLoaded;
        }

        // Constructor بدون parameters للاستخدام مع Activator.CreateInstance
        public PosOperatorsListView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
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
                    _viewModel = serviceProvider.GetRequiredService<PosOperatorsListViewModel>();
                    DataContext = _viewModel;
                    await _viewModel.InitializeAsync();
                }
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null && _viewModel != null)
            {
                _viewModel.EditSelectedOperatorCommand.Execute(null);
            }
        }
    }
}
