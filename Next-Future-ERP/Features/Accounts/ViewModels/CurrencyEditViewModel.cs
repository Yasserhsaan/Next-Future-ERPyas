using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class CurrencyEditViewModel : ObservableObject
    {
        private readonly INextCurrencyService _service;

        [ObservableProperty] 
        private NextCurrency model;

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => Model.CurrencyId == 0 ? "عملة جديدة" : "تعديل العملة";

        public CurrencyEditViewModel(INextCurrencyService service, NextCurrency model)
        {
            _service = service;
            Model = Clone(model);
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                // التحقق من صحة البيانات الأساسية
                if (string.IsNullOrWhiteSpace(Model.CurrencyNameAr))
                {
                    MessageBox.Show("الاسم العربي مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Model.CurrencyNameEn))
                {
                    MessageBox.Show("الاسم الإنجليزي مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Model.CurrencySymbol))
                {
                    MessageBox.Show("رمز العملة مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Model.CurrencyId == 0)
                {
                    Model.CreatedAt = DateTime.Now;
                    Model.CompanyId = 1;
                    await _service.AddAsync(Clone(Model));
                    MessageBox.Show("تم إضافة العملة بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Model.UpdatedAt = DateTime.Now;
                    await _service.UpdateAsync(Clone(Model));
                    MessageBox.Show("تم تحديث العملة بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private static NextCurrency Clone(NextCurrency model) => new()
        {
            CurrencyId = model.CurrencyId,
            CompanyId = model.CompanyId,
            CurrencyNameAr = model.CurrencyNameAr,
            CurrencyNameEn = model.CurrencyNameEn,
            CurrencySymbol = model.CurrencySymbol,
            FractionUnit = model.FractionUnit,
            DecimalPlaces = model.DecimalPlaces,
            IsCompanyCurrency = model.IsCompanyCurrency,
            IsForeignCurrency = model.IsForeignCurrency,
            ExchangeRate = model.ExchangeRate,
            MinExchangeRate = model.MinExchangeRate,
            MaxExchangeRate = model.MaxExchangeRate,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }
}
