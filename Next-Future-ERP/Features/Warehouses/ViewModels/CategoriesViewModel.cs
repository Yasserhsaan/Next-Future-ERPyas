using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;

namespace Next_Future_ERP.Features.Warehouses.ViewModels
{
    public partial class CategoriesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CategoryModel> categories = new();

        [ObservableProperty]
        private CategoryModel selectedCategory = new();

        [ObservableProperty]
        private ObservableCollection<CategoryModel> parentCategories = new();

        [ObservableProperty]
        private bool isExpanded = true;

        // UI Navigation properties
        public class CategoryViewModelItem : CategoryModel
        {
            public string? ParentCategoryName { get; set; }
            public ObservableCollection<CategoryViewModelItem> Children { get; set; } = new();
            public bool IsExpanded { get; set; }
            public bool IsSelected { get; set; }
        }

        [ObservableProperty]
        private ObservableCollection<CategoryViewModelItem> categoryTreeItems = new();

        private readonly ICategoryService _categoryService;

        public CategoriesViewModel(ICategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            LoadData();
        }

        private async void LoadData()
        {
            await LoadCategoriesAsync();
            await LoadParentCategoriesAsync();
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            var allCategories = await _categoryService.GetAllAsync();
            Categories.Clear();
            foreach (var category in allCategories)
                Categories.Add(category);

            // Build tree for UI
            BuildCategoryTree(allCategories);
        }

        private void BuildCategoryTree(List<CategoryModel> allCategories)
        {
            var lookup = allCategories.ToLookup(c => c.ParentCategoryID);
            
            CategoryTreeItems.Clear();
            
            foreach (var category in allCategories.Where(c => c.ParentCategoryID == null))
            {
                var treeItem = new CategoryViewModelItem
                {
                    CategoryID = category.CategoryID,
                    CategoryCode = category.CategoryCode,
                    CategoryName = category.CategoryName,
                    ParentCategoryID = category.ParentCategoryID,
                    Description = category.Description,
                    CreatedDate = category.CreatedDate,
                    ModifiedDate = category.ModifiedDate,
                    CreatedBy = category.CreatedBy,
                    ModifiedBy = category.ModifiedBy,
                    IsExpanded = true
                };
                
                BuildChildren(treeItem, lookup);
                CategoryTreeItems.Add(treeItem);
            }
        }

        private void BuildChildren(CategoryViewModelItem parent, ILookup<int?, CategoryModel> lookup)
        {
            var children = lookup[parent.CategoryID];
            foreach (var child in children)
            {
                var childItem = new CategoryViewModelItem
                {
                    CategoryID = child.CategoryID,
                    CategoryCode = child.CategoryCode,
                    CategoryName = child.CategoryName,
                    ParentCategoryID = child.ParentCategoryID,
                    Description = child.Description,
                    CreatedDate = child.CreatedDate,
                    ModifiedDate = child.ModifiedDate,
                    CreatedBy = child.CreatedBy,
                    ModifiedBy = child.ModifiedBy,
                    ParentCategoryName = parent.CategoryName,
                    IsExpanded = false
                };
                
                BuildChildren(childItem, lookup);
                parent.Children.Add(childItem);
            }
        }

        [RelayCommand]
        private async Task LoadParentCategoriesAsync()
        {
            var parents = await _categoryService.GetParentCategoriesAsync();
            ParentCategories.Clear();
            ParentCategories.Add(new CategoryModel { CategoryID = 0, CategoryName = "بدون فئة رئيسية" });
            foreach (var parent in parents)
                ParentCategories.Add(parent);
        }

        [RelayCommand]
        private void AddNew()
        {
            SelectedCategory = new CategoryModel();
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(SelectedCategory.CategoryCode) || 
                string.IsNullOrWhiteSpace(SelectedCategory.CategoryName))
            {
                System.Windows.MessageBox.Show("⚠️ يجب إدخال رمز الفئة واسم الفئة.", "تنبيه", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            await _categoryService.SaveAsync(SelectedCategory);
            await LoadCategoriesAsync();
            await LoadParentCategoriesAsync();
        }

        [RelayCommand]
        private void Cancel()
        {
            SelectedCategory = new CategoryModel();
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedCategory?.CategoryID == 0)
            {
                System.Windows.MessageBox.Show("⚠️ يرجى اختيار فئة للحذف.", "تنبيه", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var result = System.Windows.MessageBox.Show(
                $"هل أنت متأكد من حذف الفئة '{SelectedCategory?.CategoryName}'؟", 
                "تأكيد الحذف", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                await _categoryService.DeleteAsync(SelectedCategory.CategoryID);
                await LoadCategoriesAsync();
                await LoadParentCategoriesAsync();
                SelectedCategory = new CategoryModel();
            }
        }

        partial void OnSelectedCategoryChanged(CategoryModel? value)
        {
            if (value?.ParentCategoryID == 0)
                value.ParentCategoryID = null;
        }

        public void SelectCategoryFromTree(CategoryViewModelItem treeItem)
        {
            SelectedCategory = new CategoryModel
            {
                CategoryID = treeItem.CategoryID,
                CategoryCode = treeItem.CategoryCode,
                CategoryName = treeItem.CategoryName,
                ParentCategoryID = treeItem.ParentCategoryID,
                Description = treeItem.Description,
                CreatedDate = treeItem.CreatedDate,
                ModifiedDate = treeItem.ModifiedDate,
                CreatedBy = treeItem.CreatedBy,
                ModifiedBy = treeItem.ModifiedBy
            };
        }
    }
} 