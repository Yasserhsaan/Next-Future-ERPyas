using Next_Future_ERP.Features.StoreIssues.ViewModels;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Next_Future_ERP.Features.StoreIssues.Views
{
    /// <summary>
    /// Interaction logic for StoreIssuesView.xaml
    /// </summary>
    public partial class StoreIssuesView : UserControl
    {
        public StoreIssuesView()
        {
            InitializeComponent();
            
            // Set DataContext from DI
            try
            {
                DataContext = App.ServiceProvider?.GetRequiredService<StoreIssuesViewModel>();
                System.Diagnostics.Debug.WriteLine("StoreIssuesView: DataContext set successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesView: Error setting DataContext: {ex.Message}");
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            Loaded += async (s, e) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("StoreIssuesView: Loaded event triggered");
                    
                    if (DataContext is StoreIssuesViewModel vm)
                    {
                        System.Diagnostics.Debug.WriteLine("StoreIssuesView: Calling LoadAsync");
                        await vm.LoadAsync();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("StoreIssuesView: DataContext is not StoreIssuesViewModel");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"StoreIssuesView: Error in Loaded event: {ex.Message}");
                    MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is StoreIssuesViewModel vm)
            {
                vm.EditSelectedCommand.Execute(null);
            }
        }
    }
}
