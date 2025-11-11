using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.StoreIssues.Models;
using Next_Future_ERP.Features.StoreIssues.Services;
using Next_Future_ERP.Features.StoreIssues.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace Next_Future_ERP.Features.StoreIssues.Views
{
    /// <summary>
    /// Interaction logic for StoreIssueEditWindow.xaml
    /// </summary>
    public partial class StoreIssueEditWindow : FluentWindow
    {
        /// <summary>
        /// Constructor for DI - creates a new StoreIssue
        /// </summary>
        public StoreIssueEditWindow()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("StoreIssueEditWindow constructor called");
                InitializeComponent();

                Loaded += async (_, __) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("StoreIssueEditWindow Loaded event fired");
                        
                        // Create a new StoreIssue for adding
                        var newIssue = new StoreIssue
                        {
                            IssueNumber = $"SI-{DateTime.Now:yyyyMMdd-HHmmss}",
                            IssueDate = DateTime.Today,
                            CurrencyId = 1, // Default currency
                            ExchangeRate = 1.0m,
                            Status = 0, // Draft
                            CreatedBy = 1 // TODO: Get current user
                        };
                        
                        // Use DI to get ViewModel
                        var viewModelFactory = App.ServiceProvider?.GetRequiredService<Func<StoreIssue, StoreIssueEditViewModel>>();
                        if (viewModelFactory != null)
                        {
                            DataContext = viewModelFactory(newIssue);
                        }
                        else
                        {
                            // Fallback to manual creation if DI is not available
                            var service = App.ServiceProvider?.GetRequiredService<IStoreIssuesService>();
                            var destinationsService = App.ServiceProvider?.GetRequiredService<IIssueDestinationsService>();
                            DataContext = new StoreIssueEditViewModel(newIssue, service, destinationsService);
                        }
                        
                        if (DataContext is StoreIssueEditViewModel vm)
                        {
                            System.Diagnostics.Debug.WriteLine("ViewModel found, setting up CloseRequested event");
                            vm.CloseRequested += (s, ok) => 
                            {
                                try
                                {
                                    DialogResult = ok;
                                    Close();
                                }
                                catch (Exception ex)
                                {
                                    ShowErrorDialog("خطأ في إغلاق النافذة", ex);
                                }
                            };
                            
                            System.Diagnostics.Debug.WriteLine("Calling InitializeAsync...");
                            await InitializeViewModelSafely(vm);
                            System.Diagnostics.Debug.WriteLine("InitializeAsync completed");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No ViewModel found in DataContext");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowErrorDialog("خطأ في تحميل النافذة", ex);
                    }
                };
            }
            catch (Exception ex)
            {
                ShowErrorDialog("خطأ في تهيئة النافذة", ex);
            }
        }

        /// <summary>
        /// Constructor for editing existing StoreIssue
        /// </summary>
        public StoreIssueEditWindow(StoreIssue model, IStoreIssuesService service)
        {
            try
            {
                InitializeComponent();
                
                // Use DI to get ViewModel
                var viewModelFactory = App.ServiceProvider?.GetRequiredService<Func<StoreIssue, StoreIssueEditViewModel>>();
                if (viewModelFactory != null)
                {
                    DataContext = viewModelFactory(model);
                }
                else
                {
                    // Fallback to manual creation if DI is not available
                    var destinationsService = App.ServiceProvider?.GetRequiredService<IIssueDestinationsService>();
                    DataContext = new StoreIssueEditViewModel(model, service, destinationsService);
                }
                
                SetupWindow();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("خطأ في تهيئة النافذة", ex);
            }
        }

        private void SetupWindow()
        {
            Loaded += async (s, e) =>
            {
                try
                {
                    if (DataContext is StoreIssueEditViewModel vm)
                    {
                        vm.CloseRequested += (sender, result) =>
                        {
                            try
                            {
                                DialogResult = result;
                                Close();
                            }
                            catch (Exception ex)
                            {
                                ShowErrorDialog("خطأ في إغلاق النافذة", ex);
                            }
                        };
                        
                        await vm.InitializeAsync();
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorDialog("خطأ في تحميل النافذة", ex);
                }
            };
        }

        private async Task InitializeViewModelSafely(StoreIssueEditViewModel vm)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("InitializeViewModelSafely: Starting initialization");
                await vm.InitializeAsync();
                System.Diagnostics.Debug.WriteLine("InitializeViewModelSafely: Initialization completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeViewModelSafely: Error during initialization: {ex.Message}");
                ShowErrorDialog("خطأ في تهيئة البيانات", ex);
            }
        }

        private void ShowErrorDialog(string title, Exception ex)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        var errorMessage = $"{title}\n\n" +
                                         $"الخطأ: {ex.Message}\n\n" +
                                         $"التفاصيل التقنية:\n{ex.StackTrace}";
                        
                        MessageBox.Show(errorMessage, "خطأ في النظام", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception messageBoxEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"MessageBox error: {messageBoxEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Original error: {ex.Message}");
                        
                        try
                        {
                            MessageBox.Show($"خطأ: {ex.Message}", "خطأ", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine($"Critical error: {ex.Message}");
                        }
                    }
                }));
            }
            catch (Exception dispatcherEx)
            {
                System.Diagnostics.Debug.WriteLine($"Dispatcher error: {dispatcherEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Original error: {ex.Message}");
                
                try
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Critical error: {ex.Message}");
                }
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("TestButton_Click: Button clicked");
                
                if (DataContext is StoreIssueEditViewModel vm)
                {
                    vm.TestAddDetailCommand();
                }
                else
                {
                    MessageBox.Show("DataContext is not StoreIssueEditViewModel", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestButton_Click error: {ex.Message}");
                MessageBox.Show($"خطأ في الاختبار: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ComboBox_SelectionChanged: Selection changed");
                
                if (DataContext is StoreIssueEditViewModel vm)
                {
                    vm.RefreshButtonStates();
                    System.Diagnostics.Debug.WriteLine($"Button states refreshed. CanAddDetail: {vm.CanAddDetail}, CanSave: {vm.CanSave}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ComboBox_SelectionChanged error: {ex.Message}");
            }
        }
    }
}
