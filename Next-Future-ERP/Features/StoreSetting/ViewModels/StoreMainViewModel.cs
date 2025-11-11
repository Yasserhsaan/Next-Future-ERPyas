using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.StoreSetting.Models;
using Next_Future_ERP.Features.StoreSetting.Services;
using System;
using System.ComponentModel;       // DesignerProperties
using System.Threading.Tasks;
using System.Windows;              // DependencyObject

namespace Next_Future_ERP.Features.StoreSetting.ViewModels
{
    public partial class StoreMainViewModel : ObservableObject
    {
        private readonly IInventorySettingsService _service;

        [ObservableProperty] private InventorySetting? settings;

        [ObservableProperty] private int companyId = 1;
        [ObservableProperty] private int branchId = 1;

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string? statusMessage;

        public StoreMainViewModel() : this(new InventorySettingsService())
        {
        }

        public StoreMainViewModel(IInventorySettingsService service)
        {
            _service = service;

            // تهيئة فورية لواجهة المستخدم
            Settings = new InventorySetting { CompanyId = CompanyId, BranchId = BranchId };

            if (IsInDesignMode())
            {
                Settings.PricePolicy = "Retail";
                Settings.AllowNegativeStock = false;
                StatusMessage = "عرض تصميمي (Design Mode).";
            }
            else
            {
                _ = LoadAsync();
            }
        }


        private static bool IsInDesignMode()
            => DesignerProperties.GetIsInDesignMode(new DependencyObject());
        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy) return; // حارس
            try
            {
                IsBusy = true;
                StatusMessage = "جاري تحميل الإعدادات...";
                Settings = await _service.LoadAsync(CompanyId, BranchId)
                          ?? await _service.ResetDefaultsAsync(CompanyId, BranchId);
                StatusMessage = "تم التحميل.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ التحميل: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            if (Settings is null) return;

            try
            {
                IsBusy = true;
                StatusMessage = "جاري الحفظ...";
                await _service.SaveAsync(Settings);
                StatusMessage = "تم حفظ الإعدادات بنجاح.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ الحفظ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task ResetDefaultsAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "جاري إعادة الإعدادات الافتراضية...";
                Settings = await _service.ResetDefaultsAsync(CompanyId, BranchId);
                StatusMessage = "تمت الإعادة للوضع الافتراضي.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ إعادة الضبط: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
