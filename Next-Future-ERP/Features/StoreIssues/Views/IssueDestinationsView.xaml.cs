using Next_Future_ERP.Features.StoreIssues.ViewModels;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Next_Future_ERP.Features.StoreIssues.Views
{
    /// <summary>
    /// Interaction logic for IssueDestinationsView.xaml
    /// </summary>
    public partial class IssueDestinationsView : UserControl
    {
        public IssueDestinationsView()
        {
            InitializeComponent();
            
            // Set DataContext from DI
            try
            {
                DataContext = App.ServiceProvider?.GetRequiredService<IssueDestinationsViewModel>();
                System.Diagnostics.Debug.WriteLine("IssueDestinationsView: DataContext set successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsView: Error setting DataContext: {ex.Message}");
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            Loaded += async (s, e) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("IssueDestinationsView: Loaded event triggered");
                    
                    if (DataContext is IssueDestinationsViewModel vm)
                    {
                        System.Diagnostics.Debug.WriteLine("IssueDestinationsView: Calling LoadAsync");
                        await vm.LoadAsync();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("IssueDestinationsView: DataContext is not IssueDestinationsViewModel");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"IssueDestinationsView: Error in Loaded event: {ex.Message}");
                    MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is IssueDestinationsViewModel vm)
            {
                vm.EditSelectedCommand.Execute(null);
            }
        }
    }
}
