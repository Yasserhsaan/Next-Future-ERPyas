using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class AccountClassEditViewModel : ObservableObject
    {
        private readonly IAccountClassService _service;

        [ObservableProperty] 
        private AccountClass model;

        public ObservableCollection<AccountCategoryOption> CategoryOptions { get; } = new();

        [ObservableProperty] 
        private AccountCategoryOption? selectedCategoryOption;

        [ObservableProperty] 
        private string? categorySearchText;

        private bool _suppressCategorySearchRefresh;

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => Model.AccountClassId == 0 ? "تصنيف حساب جديد" : "تعديل تصنيف الحساب";

        public AccountClassEditViewModel(IAccountClassService service, AccountClass model)
        {
            _service = service;
            Model = Clone(model);
            _ = LoadLookupsAsync();
        }

        private async Task LoadLookupsAsync()
        {
            CategoryOptions.Clear();
            var options = await _service.GetAccountCategoryOptionsAsync();
            foreach (var o in options) CategoryOptions.Add(o);

            // إعداد الكومبو للقيمة الحالية
            if (!string.IsNullOrWhiteSpace(Model.CategoryKey))
            {
                var option = CategoryOptions.FirstOrDefault(o => 
                    string.Equals(o.CategoryKey, Model.CategoryKey, StringComparison.OrdinalIgnoreCase));
                if (option != null)
                {
                    _suppressCategorySearchRefresh = true;
                    try
                    {
                        SelectedCategoryOption = option;
                        CategorySearchText = option.CategoryNameAr;
                    }
                    finally
                    {
                        _suppressCategorySearchRefresh = false;
                    }
                }
            }
        }

        partial void OnCategorySearchTextChanged(string? oldValue, string? newValue)
        {
            if (_suppressCategorySearchRefresh) return;
            
            // فلترة الخيارات حسب النص المدخل
            if (string.IsNullOrWhiteSpace(newValue))
            {
                SelectedCategoryOption = null;
                Model.CategoryKey = string.Empty;
                return;
            }

            var filtered = CategoryOptions.FirstOrDefault(o => 
                o.CategoryNameAr?.Contains(newValue.Trim(), StringComparison.OrdinalIgnoreCase) == true);
            
            if (filtered != null)
            {
                SelectedCategoryOption = filtered;
                Model.CategoryKey = filtered.CategoryKey;
            }
        }

        partial void OnSelectedCategoryOptionChanged(AccountCategoryOption? oldValue, AccountCategoryOption? newValue)
        {
            if (newValue != null)
            {
                Model.CategoryKey = newValue.CategoryKey;
                
                _suppressCategorySearchRefresh = true;
                try
                {
                    CategorySearchText = newValue.CategoryNameAr;
                }
                finally
                {
                    _suppressCategorySearchRefresh = false;
                }
            }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                // التحقق من صحة البيانات قبل الحفظ
                if (string.IsNullOrWhiteSpace(Model.AccountClassAname))
                {
                    MessageBox.Show("الاسم العربي مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Model.AccountClassEname))
                {
                    MessageBox.Show("الاسم الإنجليزي مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // تنظيف البيانات
                Model.AccountClassAname = Model.AccountClassAname.Trim();
                Model.AccountClassEname = Model.AccountClassEname.Trim();
                Model.CategoryKey = Model.CategoryKey?.Trim() ?? string.Empty;

                if (Model.AccountClassId == 0)
                {
                    var id = await _service.AddAsync(Clone(Model));
                    Model.AccountClassId = id;
                    MessageBox.Show("تم إضافة التصنيف بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _service.UpdateAsync(Clone(Model));
                    MessageBox.Show("تم تحديث التصنيف بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand] 
        public void Cancel() => CloseRequested?.Invoke(this, false);

        private static AccountClass Clone(AccountClass model) => new()
        {
            AccountClassId = model.AccountClassId,
            AccountClassAname = model.AccountClassAname,
            AccountClassEname = model.AccountClassEname,
            CategoryKey = model.CategoryKey
        };
    }
}
