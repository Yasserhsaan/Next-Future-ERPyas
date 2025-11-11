using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.StoreIssues.Models;
using Next_Future_ERP.Features.StoreIssues.Services;
using Next_Future_ERP.Features.StoreIssues.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.StoreIssues.ViewModels
{
    public partial class StoreIssuesViewModel : ObservableObject
    {
        private readonly IStoreIssuesService _service;

        [ObservableProperty] private ObservableCollection<StoreIssue> issues = new();
        [ObservableProperty] private StoreIssue? selectedIssue;
        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private bool isLoading;

        public StoreIssuesViewModel(IStoreIssuesService service)
        {
            _service = service;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("StoreIssuesViewModel.LoadAsync: Starting load operation");

                var issues = await _service.GetAllAsync();
                Issues.Clear();
                foreach (var issue in issues)
                {
                    Issues.Add(issue);
                }

                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.LoadAsync: Loaded {Issues.Count} issues");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.LoadAsync: Error: {ex.Message}");
                System.Windows.MessageBox.Show($"خطأ في تحميل مستندات الصرف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.SearchAsync: Searching for '{SearchText}'");

                var issues = await _service.GetAllAsync(SearchText);
                Issues.Clear();
                foreach (var issue in issues)
                {
                    Issues.Add(issue);
                }

                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.SearchAsync: Found {Issues.Count} issues");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.SearchAsync: Error: {ex.Message}");
                System.Windows.MessageBox.Show($"خطأ في البحث عن مستندات الصرف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void ClearSearch()
        {
            SearchText = string.Empty;
            _ = LoadAsync();
        }

        [RelayCommand]
        public void AddNew()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("StoreIssuesViewModel.AddNew: Opening add window");

                var newIssue = new StoreIssue
                {
                    CompanyId = 1, // TODO: Get from current context
                    BranchId = 1,  // TODO: Get from current context
                    IssueDate = DateTime.Today,
                    CurrencyId = 1, // TODO: Get default currency
                    ExchangeRate = 1.0m,
                    Status = 0, // Draft
                    CreatedBy = 1 // TODO: Get current user
                };

                // Use DI to create the window
                var windowFactory = App.ServiceProvider?.GetRequiredService<Func<StoreIssue, StoreIssueEditWindow>>();
                if (windowFactory != null)
                {
                    var editWindow = windowFactory(newIssue);
                    editWindow.Owner = System.Windows.Application.Current.MainWindow;
                    
                    if (editWindow.ShowDialog() == true)
                    {
                        _ = LoadAsync(); // Refresh the list
                    }
                }
                else
                {
                    // Fallback to manual creation
                    var editWindow = new StoreIssueEditWindow(newIssue, _service);
                    editWindow.Owner = System.Windows.Application.Current.MainWindow;
                    
                    if (editWindow.ShowDialog() == true)
                    {
                        _ = LoadAsync(); // Refresh the list
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.AddNew: Error: {ex.Message}");
                System.Windows.MessageBox.Show($"خطأ في فتح نافذة الإضافة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void EditSelected()
        {
            if (SelectedIssue == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.EditSelected: Editing issue ID: {SelectedIssue.IssueId}");

                // Use DI to create the window
                var windowFactory = App.ServiceProvider?.GetRequiredService<Func<StoreIssue, StoreIssueEditWindow>>();
                if (windowFactory != null)
                {
                    var editWindow = windowFactory(SelectedIssue);
                    editWindow.Owner = System.Windows.Application.Current.MainWindow;
                    
                    if (editWindow.ShowDialog() == true)
                    {
                        _ = LoadAsync(); // Refresh the list
                    }
                }
                else
                {
                    // Fallback to manual creation
                    var editWindow = new StoreIssueEditWindow(SelectedIssue, _service);
                    editWindow.Owner = System.Windows.Application.Current.MainWindow;
                    
                    if (editWindow.ShowDialog() == true)
                    {
                        _ = LoadAsync(); // Refresh the list
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.EditSelected: Error: {ex.Message}");
                System.Windows.MessageBox.Show($"خطأ في فتح نافذة التعديل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task DeleteSelectedAsync()
        {
            if (SelectedIssue == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.DeleteSelectedAsync: Deleting issue ID: {SelectedIssue.IssueId}");

                var result = System.Windows.MessageBox.Show(
                    $"هل أنت متأكد من حذف مستند الصرف '{SelectedIssue.IssueNumber}'؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _service.DeleteAsync(SelectedIssue.IssueId);
                    _ = LoadAsync(); // Refresh the list
                    
                    System.Windows.MessageBox.Show(
                        "تم حذف مستند الصرف بنجاح.",
                        "تم الحذف",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.DeleteSelectedAsync: Error: {ex.Message}");
                
                System.Windows.MessageBox.Show(
                    $"خطأ في حذف مستند الصرف:\n\n{ex.Message}",
                    "خطأ في الحذف",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task PostSelectedAsync()
        {
            if (SelectedIssue == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.PostSelectedAsync: Posting issue ID: {SelectedIssue.IssueId}");

                var result = System.Windows.MessageBox.Show(
                    $"هل أنت متأكد من ترحيل مستند الصرف '{SelectedIssue.IssueNumber}'؟\n\nسيتم تأثيره على المخزون والمحاسبة.",
                    "تأكيد الترحيل",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _service.PostAsync(SelectedIssue.IssueId);
                    if (success)
                    {
                        _ = LoadAsync(); // Refresh the list
                        
                        System.Windows.MessageBox.Show(
                            "تم ترحيل مستند الصرف بنجاح.",
                            "تم الترحيل",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.PostSelectedAsync: Error: {ex.Message}");
                
                System.Windows.MessageBox.Show(
                    $"خطأ في ترحيل مستند الصرف:\n\n{ex.Message}",
                    "خطأ في الترحيل",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task CancelSelectedAsync()
        {
            if (SelectedIssue == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.CancelSelectedAsync: Canceling issue ID: {SelectedIssue.IssueId}");

                var result = System.Windows.MessageBox.Show(
                    $"هل أنت متأكد من إلغاء مستند الصرف '{SelectedIssue.IssueNumber}'؟",
                    "تأكيد الإلغاء",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _service.CancelAsync(SelectedIssue.IssueId);
                    if (success)
                    {
                        _ = LoadAsync(); // Refresh the list
                        
                        System.Windows.MessageBox.Show(
                            "تم إلغاء مستند الصرف بنجاح.",
                            "تم الإلغاء",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesViewModel.CancelSelectedAsync: Error: {ex.Message}");
                
                System.Windows.MessageBox.Show(
                    $"خطأ في إلغاء مستند الصرف:\n\n{ex.Message}",
                    "خطأ في الإلغاء",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Properties for UI binding
        public bool CanEdit => SelectedIssue?.Status == 0; // Only Draft
        public bool CanDelete => SelectedIssue?.Status == 0; // Only Draft
        public bool CanPost => SelectedIssue?.Status == 0; // Only Draft
        public bool CanCancel => SelectedIssue?.Status != 2; // Not Canceled
    }
}
