using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Items.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Items.ViewModels
{
    public partial class ItemComponentsViewModel : ObservableObject
    {
        private readonly IItemComponentsService _service;
        private readonly IUnitsLookupService _unitsService;

        [ObservableProperty]
        private int? _currentItemId;
        
        partial void OnCurrentItemIdChanged(int? oldValue, int? newValue)
        {
            // تحميل البيانات بشكل آمن
            _ = LoadAsync();
        }

        [ObservableProperty]
        private ObservableCollection<ItemComponent> _components = new();

        [ObservableProperty]
        private ItemComponent? _edit;

        [ObservableProperty]
        private ObservableCollection<KeyValuePair<int, string>> _itemOptions = new();

        [ObservableProperty]
        private ObservableCollection<KeyValuePair<int, string>> _unitOptions = new();

        public ItemComponentsViewModel(IItemComponentsService service, IUnitsLookupService unitsService)
        {
            _service = service;
            _unitsService = unitsService;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                await LoadItemOptionsAsync();
                await LoadUnitOptionsAsync();

                if (CurrentItemId.HasValue && CurrentItemId > 0)
                {
                    var list = await _service.GetByParentItemAsync(CurrentItemId.Value);
                    Components.Clear();
                    foreach (var item in list)
                        Components.Add(item);
                }
                else
                {
                    Components.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadItemOptionsAsync()
        {
            ItemOptions.Clear();
            // Load all items except the current one (to prevent self-reference)
            var items = await App.ServiceProvider.GetRequiredService<IItemsService>().GetAllAsync();
            foreach (var item in items.Where(x => x.ItemID != CurrentItemId))
            {
                ItemOptions.Add(new KeyValuePair<int, string>(item.ItemID, item.ItemName));
            }
        }

        private async Task LoadUnitOptionsAsync()
        {
            UnitOptions.Clear();
            var units = await _unitsService.GetAllAsync();
            foreach (var unit in units)
            {
                UnitOptions.Add(new KeyValuePair<int, string>(unit.UnitID, unit.UnitName));
            }
        }

        [RelayCommand]
        public void New()
        {
            Edit = new ItemComponent
            {
                ItemComponentID = 0,
                ParentItemID = CurrentItemId ?? 0,
                ComponentItemID = 0,
                UnitID = 0,
                Quantity = 0m
            };
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
                if (Edit.ComponentItemID <= 0)
                {
                    MessageBox.Show("يرجى اختيار المكون.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (Edit.UnitID <= 0)
                {
                    MessageBox.Show("يرجى اختيار الوحدة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (Edit.Quantity <= 0)
                {
                    MessageBox.Show("يرجى إدخال كمية أكبر من صفر.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Edit.ParentItemID = CurrentItemId.Value;

                if (Edit.ItemComponentID == 0)
                    Edit.ItemComponentID = await _service.AddAsync(Edit);
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
        public async Task DeleteAsync(ItemComponent component)
        {
            try
            {
                if (component == null) return;

                var result = MessageBox.Show($"هل تريد حذف المكون '{component.ComponentItemName}'؟", 
                    "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    await _service.DeleteAsync(component.ItemComponentID);
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
