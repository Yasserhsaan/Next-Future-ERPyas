using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.StoreIssues.Models;
using Next_Future_ERP.Features.StoreIssues.Services;
using Next_Future_ERP.Features.StoreIssues.ViewModels;
using System;
using System.Windows;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace Next_Future_ERP.Features.StoreIssues.Views
{
    /// <summary>
    /// Interaction logic for IssueDestinationEditWindow.xaml
    /// </summary>
    public partial class IssueDestinationEditWindow : FluentWindow
    {
        public IssueDestinationEditWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is IssueDestinationEditViewModel vm)
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
        }

        public IssueDestinationEditWindow(IssueDestination model, IIssueDestinationsService service, AccountsService accountsService, ICostCentersService costCentersService)
        {
            try
            {
                InitializeComponent();
                DataContext = new IssueDestinationEditViewModel(model, service, accountsService, costCentersService);
            }
            catch (Exception ex)
            {
                ShowErrorDialog("خطأ في تهيئة النافذة", ex);
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
    }
}
