using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Accounts.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Items.ViewModels
{
    public partial class ItemSuppliersViewModel : ObservableObject
    {
        private readonly IItemSuppliersService _service;
        private readonly Next_Future_ERP.Features.Suppliers.Services.ISuppliersService _suppliersService;

        [ObservableProperty]
        private int? currentItemId;
        
        partial void OnCurrentItemIdChanged(int? oldValue, int? newValue)
        {
            // تحميل البيانات بشكل آمن
            _ = LoadAsync();
        }

        [ObservableProperty]
        private ObservableCollection<ItemSupplier> suppliers = new();

        [ObservableProperty]
        private ItemSupplier? edit = new();

        public ObservableCollection<KeyValuePair<int, string>> SupplierOptions { get; } = new();
        public ObservableCollection<KeyValuePair<string, string>> CurrencyOptions { get; } = new();

        private readonly NextCurrencyService _currencyService;

        public ItemSuppliersViewModel(IItemSuppliersService service, ISuppliersService suppliersService, NextCurrencyService currencyService)
        {
            _service = service;
            _suppliersService = suppliersService;
            _currencyService = currencyService;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                await LoadSupplierOptionsAsync();
                if (CurrentItemId == null || CurrentItemId <= 0) 
                {
                    await Application.Current.Dispatcher.InvokeAsync(() => Suppliers.Clear());
                    return;
                }
                
                var list = await _service.GetByItemAsync(CurrentItemId.Value);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Suppliers.Clear();
                    foreach (var s in list) Suppliers.Add(s);
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        [RelayCommand]
        public void New()
        {
            Edit = new ItemSupplier
            {
                ItemID = CurrentItemId ?? 0,
                SupplierPrice = 0m,
                CurrencyCode = null,
                IsPrimarySupplier = false
            };
        }

        private async Task LoadSupplierOptionsAsync()
        {
            var list = await _suppliersService.GetAllAsync();
            var currencies = await _currencyService.GetAllAsync();
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                SupplierOptions.Clear();
                foreach (var s in list)
                {
                    SupplierOptions.Add(new KeyValuePair<int, string>(s.SupplierID, s.SupplierName));
                }
                
                CurrencyOptions.Clear();
                foreach (var c in currencies)
                {
                    var key = string.IsNullOrWhiteSpace(c.CurrencySymbol) ? c.CurrencyId.ToString() : c.CurrencySymbol;
                    var name = string.IsNullOrWhiteSpace(c.CurrencyNameAr) ? c.CurrencyNameEn : c.CurrencyNameAr;
                    CurrencyOptions.Add(new KeyValuePair<string, string>(key, $"{name} ({c.CurrencySymbol})"));
                }
            });
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                if (Edit == null || CurrentItemId == null || CurrentItemId <= 0)
                {
                    MessageBox.Show("اختر صنفاً أولاً.");
                    return;
                }
                if (Edit.SupplierID <= 0)
                {
                    MessageBox.Show("يرجى اختيار المورد.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(Edit.CurrencyCode))
                {
                    MessageBox.Show("يرجى اختيار العملة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Edit.ItemID = CurrentItemId.Value;

                if (Edit.ItemSupplierID == 0)
                    Edit.ItemSupplierID = await _service.AddAsync(Edit);
                else
                    await _service.UpdateAsync(Edit);

                await LoadAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.GetBaseException().Message;
                MessageBox.Show(inner, "خطأ قاعدة البيانات", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task DeleteAsync(ItemSupplier row)
        {
            if (row == null) return;
            await _service.DeleteAsync(row.ItemSupplierID);
            await LoadAsync();
        }
    }
}


