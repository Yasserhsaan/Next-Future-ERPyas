// SalesSettingsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Features.Sales.Services;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Sales.ViewModels
{
    public class SalesSettingsViewModel : ObservableObject, IDisposable
    {
        private readonly SalesSettingService _service;
        private bool _disposed = false;

        private SalesSetting _salesSetting = new();
        public SalesSetting SalesSetting
        {
            get => _salesSetting;
            set => SetProperty(ref _salesSetting, value);
        }
        // إضافة قائمة طرق الدفع
        public List<string> PaymentMethods { get; } = new()
    {
        "Cash",
        "Card",
        "Wallet"
    };
        public ICommand SaveCommand { get; }
        public ICommand ResetCommand { get; }

        // قائمة خيارات ترقيم الفواتير
        public List<KeyValuePair<string, string>> InvoiceNumberingOptions { get; } = new()
        {
            new KeyValuePair<string, string>("INV-(yyyy)-(####)", "INV-(السنة)-(رقم تسلسلي)"),
            new KeyValuePair<string, string>("INV-(mm-yyyy)-(####)", "INV-(الشهر-السنة)-(رقم تسلسلي)"),
            new KeyValuePair<string, string>("INV-(dd-mm-yyyy)-(####)", "INV-(اليوم-الشهر-السنة)-(رقم تسلسلي)")
        };

        public SalesSettingsViewModel()
        {
            _service = new SalesSettingService();
            SaveCommand = new RelayCommand(Save);
            ResetCommand = new RelayCommand(Reset);

            LoadSettings();
        }
        // InventorySettingService.cs

        // SalesSettingsViewModel.cs
        private async void LoadSettings()
        {
            try
            {
                // استخدام قيم افتراضية للشركة والفرع - يجب تعديلها حسب النظام الخاص بك
                int companyId = 1; // يجب الحصول من النظام
                int branchId = 1;  // يجب الحصول من النظام

                SalesSetting = await _service.GetByCompanyAndBranchAsync(companyId, branchId);

                if (SalesSetting == null)
                {
                    MessageBox.Show("لم يتم العثور على إعدادات المبيعات.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في تحميل الإعدادات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Save()
        {
            try
            {
                if (SalesSetting == null)
                {
                    MessageBox.Show("إعدادات المبيعات غير متوفرة.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await _service.SaveAsync(SalesSetting);
                MessageBox.Show("تم حفظ الإعدادات بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reset()
        {
            var result = MessageBox.Show("هل أنت متأكد من إعادة تعيين الإعدادات؟", "تأكيد",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SalesSetting = new SalesSetting
                {
                    CompanyId = SalesSetting.CompanyId,
                    BranchId = SalesSetting.BranchId,
                    AutoPostInvoice = false,
                    AllowDiscount = false,
                    PosEnabled = false,
                    PosAutoPrint = false
                };
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _service?.Dispose();
                _disposed = true;
            }
        }

        ~SalesSettingsViewModel()
        {
            Dispose(false);
        }
    }
}