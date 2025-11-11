using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class ExchangeRateEditViewModel : ObservableObject
    {
        private readonly ICurrencyExchangeRateService _service;

        [ObservableProperty] 
        private CurrencyExchangeRate model;

        public ObservableCollection<NextCurrency> Currencies { get; } = new();

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => Model.Id == 0 ? "سعر صرف جديد" : "تعديل سعر الصرف";

        public ExchangeRateEditViewModel(ICurrencyExchangeRateService service, CurrencyExchangeRate model)
        {
            _service = service;
            Model = Clone(model);
            _ = LoadCurrenciesAsync();
        }

        private async Task LoadCurrenciesAsync()
        {
            Currencies.Clear();
            var currencies = await _service.GetActiveCurrenciesAsync();
            foreach (var currency in currencies.OrderBy(x => x.CurrencyNameAr))
            {
                Currencies.Add(currency);
            }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                // التحقق من صحة البيانات الأساسية
                if (Model.CurrencyId <= 0)
                {
                    MessageBox.Show("العملة مطلوبة.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Model.ExchangeRate <= 0)
                {
                    MessageBox.Show("سعر الصرف يجب أن يكون أكبر من صفر.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Model.DateExchangeEnd.HasValue && Model.DateExchangeEnd < Model.DateExchangeStart)
                {
                    MessageBox.Show("تاريخ انتهاء الصرف يجب أن يكون بعد تاريخ البدء.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Model.Id == 0)
                {
                    await _service.AddAsync(Clone(Model));
                    MessageBox.Show("تم إضافة سعر الصرف بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _service.UpdateAsync(Clone(Model));
                    MessageBox.Show("تم تحديث سعر الصرف بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private static CurrencyExchangeRate Clone(CurrencyExchangeRate model) => new()
        {
            Id = model.Id,
            CurrencyId = model.CurrencyId,
            ExchangeRate = model.ExchangeRate,
            DateExchangeStart = model.DateExchangeStart,
            DateExchangeEnd = model.DateExchangeEnd,
            Status = model.Status,
            Currency = model.Currency
        };
    }
}
