using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Permissions.Models;
using Next_Future_ERP.Features.Permissions.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Permissions.ViewModels
{
    public partial class MenuEditorViewModel : ObservableObject
    {
        private readonly IPermissionService _permissionService;

        [ObservableProperty]
        private ObservableCollection<MenuForm> menuForms = new();

        [ObservableProperty]
        private ObservableCollection<MenuForm> parentMenuForms = new();

        [ObservableProperty]
        private MenuForm? selectedMenuForm;

        [ObservableProperty]
        private MenuForm newMenuForm = new();

        [ObservableProperty]
        private bool isAddingNew = false;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool isLoadingMenuForms = false;

        [ObservableProperty]
        private bool isSaving = false;

        [ObservableProperty]
        private bool isDeleting = false;

        [ObservableProperty]
        private string loadingMessage = string.Empty;

        [ObservableProperty]
        private string searchText = string.Empty;

        public MenuEditorViewModel(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [RelayCommand]
        private async Task LoadMenuFormsAsync()
        {
            try
            {
                IsLoading = true;
                IsLoadingMenuForms = true;
                LoadingMessage = "تحميل القوائم...";
                
                MenuForms.Clear();
                ParentMenuForms.Clear();

                LoadingMessage = "جلب جميع القوائم...";
                var allMenuForms = await _permissionService.GetAllMenuFormsAsync();
                
                LoadingMessage = "جلب شجرة القوائم...";
                var treeMenuForms = await _permissionService.GetMenuFormsTreeAsync();

                LoadingMessage = "تنظيم البيانات...";
                foreach (var menuForm in allMenuForms)
                {
                    MenuForms.Add(menuForm);
                }

                // Add "No Parent" option
                ParentMenuForms.Add(new MenuForm { MenuFormCode = 0, MenuName = "بدون أب", MenuArabicName = "بدون أب" });

                foreach (var menuForm in allMenuForms.Where(m => m.IsParent))
                {
                    ParentMenuForms.Add(menuForm);
                }
                
                LoadingMessage = "تم تحميل القوائم بنجاح";
            }
            catch (Exception ex)
            {
                LoadingMessage = $"خطأ أثناء تحميل القوائم: {ex.Message}";
                MessageBox.Show($"❌ خطأ أثناء تحميل القوائم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                IsLoadingMenuForms = false;
                // Clear message after a short delay
                await Task.Delay(2000);
                LoadingMessage = string.Empty;
            }
        }

        [RelayCommand]
        private void StartAddingNew()
        {
            NewMenuForm = new MenuForm
            {
                MenuFormCode = 0,
                MenuFormParent = null,
                MenuName = string.Empty,
                MenuArabicName = string.Empty,
                ProgramExecutable = string.Empty,
                Visible = 1,
                NSync = 0
            };
            IsAddingNew = true;
            SelectedMenuForm = null;
        }

        [RelayCommand]
        private void CancelAdding()
        {
            IsAddingNew = false;
            NewMenuForm = new MenuForm();
        }

        [RelayCommand]
        private async Task SaveMenuFormAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewMenuForm.MenuName))
                {
                    MessageBox.Show("⚠️ يجب إدخال اسم القائمة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsLoading = true;
                IsSaving = true;

                if (NewMenuForm.MenuFormCode == 0)
                {
                    // Adding new
                    LoadingMessage = "إضافة القائمة الجديدة...";
                    var success = await _permissionService.AddMenuFormAsync(NewMenuForm);
                    if (success)
                    {
                        LoadingMessage = "تحديث قائمة القوائم...";
                        MessageBox.Show("✅ تم إضافة القائمة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadMenuFormsAsync();
                        CancelAdding();
                    }
                }
                else
                {
                    // Updating existing
                    LoadingMessage = "تحديث القائمة...";
                    var success = await _permissionService.UpdateMenuFormAsync(NewMenuForm);
                    if (success)
                    {
                        LoadingMessage = "تحديث قائمة القوائم...";
                        MessageBox.Show("✅ تم تحديث القائمة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadMenuFormsAsync();
                        CancelAdding();
                    }
                }
            }
            catch (Exception ex)
            {
                LoadingMessage = $"خطأ أثناء حفظ القائمة: {ex.Message}";
                MessageBox.Show($"❌ خطأ أثناء حفظ القائمة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                IsSaving = false;
                LoadingMessage = string.Empty;
            }
        }

        [RelayCommand]
        private void EditMenuForm()
        {
            if (SelectedMenuForm != null)
            {
                NewMenuForm = new MenuForm
                {
                    MenuFormCode = SelectedMenuForm.MenuFormCode,
                    MenuFormParent = SelectedMenuForm.MenuFormParent,
                    MenuName = SelectedMenuForm.MenuName,
                    MenuArabicName = SelectedMenuForm.MenuArabicName,
                    ProgramExecutable = SelectedMenuForm.ProgramExecutable,
                    Visible = SelectedMenuForm.Visible,
                    NSync = SelectedMenuForm.NSync
                };
                IsAddingNew = true;
            }
        }

        [RelayCommand]
        private async Task DeleteMenuFormAsync()
        {
            if (SelectedMenuForm == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف القائمة '{SelectedMenuForm.DisplayName}'؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    IsDeleting = true;
                    LoadingMessage = "حذف القائمة...";
                    
                    var success = await _permissionService.DeleteMenuFormAsync(SelectedMenuForm.MenuFormCode);
                    if (success)
                    {
                        LoadingMessage = "تحديث قائمة القوائم...";
                        MessageBox.Show("✅ تم حذف القائمة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadMenuFormsAsync();
                        SelectedMenuForm = null;
                    }
                }
                catch (Exception ex)
                {
                    LoadingMessage = $"خطأ أثناء حذف القائمة: {ex.Message}";
                    MessageBox.Show($"❌ خطأ أثناء حذف القائمة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                    IsDeleting = false;
                    LoadingMessage = string.Empty;
                }
            }
        }

        [RelayCommand]
        private void SearchMenuForms()
        {
            // Implement search functionality
            // This could filter the MenuForms collection based on SearchText
        }

        partial void OnSelectedMenuFormChanged(MenuForm? value)
        {
            // Handle selection change
            if (value != null)
            {
                // You can add logic here to show details or enable/disable buttons
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            // Implement real-time search if needed
            SearchMenuForms();
        }
    }
}
