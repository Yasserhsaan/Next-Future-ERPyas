using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Items.ViewModels
{
    public partial class InventoryBalanceViewModel : ObservableObject
    {
        private readonly IInventoryBalanceService _service;
        private readonly IWarehouseService _warehouseService;
        private readonly IUnitsLookupService _unitsService;

        [ObservableProperty]
        private int? currentItemId;

        [ObservableProperty]
        private ObservableCollection<Warehouse> warehouses = new();

        [ObservableProperty]
        private ObservableCollection<UnitModel> units = new();

        partial void OnCurrentItemIdChanged(int? oldValue, int? newValue)
        {
            // تحميل البيانات بشكل آمن
            _ = LoadAsync();
        }

        [ObservableProperty]
        private ObservableCollection<InventoryBalance> balances = new();

        [ObservableProperty]
        private InventoryBalance? edit = new();

        // خصائص للأسماء في DataGrid
        public string GetWarehouseName(int warehouseId)
        {
            var warehouse = Warehouses.FirstOrDefault(w => w.WarehouseID == warehouseId);
            return warehouse?.WarehouseName ?? warehouseId.ToString();
        }

        public string GetUnitName(int unitId)
        {
            var unit = Units.FirstOrDefault(u => u.UnitID == unitId);
            return unit?.UnitName ?? unitId.ToString();
        }

        public InventoryBalanceViewModel(IInventoryBalanceService service, IWarehouseService warehouseService, IUnitsLookupService unitsService)
        {
            _service = service;
            _warehouseService = warehouseService;
            _unitsService = unitsService;
            
            // تحميل البيانات المرجعية بشكل آمن
            _ = LoadLookupsAsync();
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                // تحميل المخازن
                var warehousesList = await _warehouseService.GetAllAsync();
                
                // تحميل الوحدات
                var unitsList = await _unitsService.GetAllAsync();
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Warehouses.Clear();
                    foreach (var warehouse in warehousesList)
                    {
                        Warehouses.Add(warehouse);
                    }

                    Units.Clear();
                    foreach (var unit in unitsList)
                    {
                        Units.Add(unit);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"خطأ في تحميل البيانات المرجعية: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                if (CurrentItemId == null || CurrentItemId <= 0) return;
                
                var list = await _service.GetByItemAsync(CurrentItemId.Value);
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Balances.Clear();
                    foreach (var b in list) Balances.Add(b);
                    if (Edit == null) New();
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
            Edit = new InventoryBalance
            {
                ItemID = CurrentItemId ?? 0,
                Quantity = 0m,
                AvgCost = 0m,
                LastCost = 0m,
                FtCost = 0m,
                LastUpdate = DateTime.Now
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
                Edit.ItemID = CurrentItemId.Value;
                Edit.LastUpdate = DateTime.Now;

                if (Edit.BalanceID == 0)
                {
                    var savedBalance = await _service.AddAsync(Edit);
                    Edit.BalanceID = savedBalance.BalanceID;
                }
                else
                    await _service.UpdateAsync(Edit);

                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task DeleteAsync(InventoryBalance row)
        {
            if (row == null) return;
            await _service.DeleteAsync(row.BalanceID);
            await LoadAsync();
        }
    }
}
