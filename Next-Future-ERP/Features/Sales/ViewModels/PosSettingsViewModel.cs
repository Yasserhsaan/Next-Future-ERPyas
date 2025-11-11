// PosSettingsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Sales.Services;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Sales.ViewModels
{
    public class PosSettingsViewModel : ObservableObject, IDisposable
    {
        private readonly PosSettingService _service;
        private bool _disposed = false;

        private PosSetting _posSetting = new();
        public PosSetting PosSetting
        {
            get => _posSetting;
            set => SetProperty(ref _posSetting, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ShowGeneralSettingsTabCommand { get; }
        public ICommand ShowInvoiceSettingsTabCommand { get; }
        public ICommand ShowAdvancedSettingsTabCommand { get; }

        // Properties للتحكم في عرض الأقسام
        private bool _showGeneralSettings = true;
        public bool ShowGeneralSettings
        {
            get => _showGeneralSettings;
            set => SetProperty(ref _showGeneralSettings, value);
        }

        private bool _showInvoiceSettings = false;
        public bool ShowInvoiceSettings
        {
            get => _showInvoiceSettings;
            set => SetProperty(ref _showInvoiceSettings, value);
        }

        private bool _showAdvancedSettings = false;
        public bool ShowAdvancedSettings
        {
            get => _showAdvancedSettings;
            set => SetProperty(ref _showAdvancedSettings, value);
        }
        // قائمة الحقول الإجبارية للعرض
        public List<string> RequiredFields { get; } = new()
    {
        "CompanyId",
        "BranchId",
        "EnableEInvoice",
        "QrCodeOnInvoice",
        "ShowVatBreakdown",
        "PrintInvoiceAuto",
        "SearchForItems"
    };
        // قوائم الخيارات
        public List<KeyValuePair<int?, string>> PricingOptions { get; } = new()
        {
            new KeyValuePair<int?, string>(null, "اختر طريقة التسعير"),
            new KeyValuePair<int?, string>(1, "سعر البيع 1"),
            new KeyValuePair<int?, string>(2, "سعر البيع 2"),
            new KeyValuePair<int?, string>(3, "سعر البيع 3")
        };

        public List<KeyValuePair<int?, string>> RoundOptions { get; } = new()
        {
            new KeyValuePair<int?, string>(null, "اختر طريقة التقريب"),
            new KeyValuePair<int?, string>(1, "تقريب لأقرب 5"),
            new KeyValuePair<int?, string>(2, "تقريب لأقرب 10"),
            new KeyValuePair<int?, string>(3, "تقريب لأقرب 100")
        };

        public List<KeyValuePair<int?, string>> PaymentMethodOptions { get; } = new()
        {
            new KeyValuePair<int?, string>(null, "اختر طريقة الدفع"),
            new KeyValuePair<int?, string>(1, "نقد"),
            new KeyValuePair<int?, string>(2, "بطاقة ائتمان"),
            new KeyValuePair<int?, string>(3, "تحويل بنكي")
        };

        public List<KeyValuePair<int?, string>> SequenceTypeOptions { get; } = new()
        {
            new KeyValuePair<int?, string>(null, "اختر نوع التسلسل"),
            new KeyValuePair<int?, string>(1, "تسلسلي"),
            new KeyValuePair<int?, string>(2, "بالتاريخ"),
            new KeyValuePair<int?, string>(3, "عشوائي")
        };

        public PosSettingsViewModel()
        {
            _service = new PosSettingService();
            SaveCommand = new RelayCommand(Save);
            ResetCommand = new RelayCommand(Reset);
            ShowGeneralSettingsTabCommand = new RelayCommand(ShowGeneralSettingsTab);
            ShowInvoiceSettingsTabCommand = new RelayCommand(ShowInvoiceSettingsTab);
            ShowAdvancedSettingsTabCommand = new RelayCommand(ShowAdvancedSettingsTab);

            LoadSettings();
        }

        private async void LoadSettings()
        {
            try
            {
                // استخدام قيم افتراضية للشركة والفرع - يجب تعديلها حسب النظام الخاص بك
                int companyId = 1; // يجب الحصول من النظام
                int branchId = 1;  // يجب الحصول من النظام

                PosSetting = await _service.GetByCompanyAndBranchAsync(companyId, branchId);

                if (PosSetting == null)
                {
                    MessageBox.Show("لم يتم العثور على إعدادات نقاط البيع.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في تحميل الإعدادات: {ex.Message}\n\nتفاصيل الخطأ الداخلي:\n{ex.InnerException?.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Save()
        {
            try
            {
                if (PosSetting == null)
                {
                    MessageBox.Show("إعدادات نقاط البيع غير متوفرة.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // التحقق من الحقول الإجبارية منطقياً
                if (string.IsNullOrWhiteSpace(PosSetting.DefaultPosUser))
                {
                    var result = MessageBox.Show("المستخدم الافتراضي لنقطة البيع غير محدد. هل تريد المتابعة؟",
                                               "تحذير", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                        return;
                }

                await _service.SaveAsync(PosSetting);
                MessageBox.Show("تم حفظ الإعدادات بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}\n\nتفاصيل الخطأ الداخلي:\n{ex.InnerException?.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reset()
        {
            var result = MessageBox.Show("هل أنت متأكد من إعادة تعيين الإعدادات؟", "تأكيد",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                PosSetting = new PosSetting
                {
                    CompanyId = PosSetting.CompanyId,
                    BranchId = PosSetting.BranchId,
                    EnableEInvoice = false,
                    QrCodeOnInvoice = false,
                    ShowVatBreakdown = false,
                    PrintInvoiceAuto = false,
                    SearchForItems = true
                };
            }
        }

        // Methods للتحكم في عرض الأقسام
        public void ShowGeneralSettingsTab()
        {
            ShowGeneralSettings = true;
            ShowInvoiceSettings = false;
            ShowAdvancedSettings = false;
        }

        public void ShowInvoiceSettingsTab()
        {
            ShowGeneralSettings = false;
            ShowInvoiceSettings = true;
            ShowAdvancedSettings = false;
        }

        public void ShowAdvancedSettingsTab()
        {
            ShowGeneralSettings = false;
            ShowInvoiceSettings = false;
            ShowAdvancedSettings = true;
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

        ~PosSettingsViewModel()
        {
            Dispose(false);
        }
    }
}