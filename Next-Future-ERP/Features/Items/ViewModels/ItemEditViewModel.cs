using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Items.Views;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Items.ViewModels
{
    public partial class ItemEditViewModel : ObservableObject
    {
        private readonly IItemsService _service;
        private readonly IUnitsLookupService _units;
        private readonly ICategoriesLookupService _cats;
        private readonly IValuationGroupService _valGroups;
        private readonly IItemTypeService _itemTypeService;
        private readonly IItemCostsService _itemCostsService;
        private readonly IItemPricesService _itemPricesService;
        private readonly IItemBatchesService _itemBatchesService;
        private readonly IItemComponentsService _itemComponentsService;
        private readonly IItemSuppliersService _itemSuppliersService;
        private readonly ISuppliersService _suppliersService;
        private readonly NextCurrencyService _currencyService;
        private readonly IInventoryBalanceService _inventoryBalanceService;
        private readonly IWarehouseService _warehouseService;

    [ObservableProperty] private Item model;
    
    partial void OnModelChanged(Item value)
    {
        // تحديث ViewModels عند تغيير Model (فقط إذا لم يكن هذا التغيير الأولي)
        // لا نحتاج لاستدعاء UpdateViewModels هنا لأنها ستُستدعى في Constructor
    }
    public ObservableCollection<UnitModel> AllUnits { get; } = new();
    public ObservableCollection<CategoryModel> AllCategories { get; } = new();
    public ObservableCollection<ValuationGroup> AllValuationGroups { get; } = new();
    public ObservableCollection<ItemType> AllItemTypes { get; } = new();
    public ObservableCollection<ItemUnit> ItemUnits { get; } = new();

    // ViewModels للتبويبات المتقدمة
    public ItemCostsViewModel ItemCostsVM { get; }
    public ItemPricesViewModel ItemPricesVM { get; }
    public ItemSuppliersViewModel ItemSuppliersVM { get; }
    public ItemBatchesViewModel ItemBatchesVM { get; }
    public ItemComponentsViewModel ItemComponentsVM { get; }
    public InventoryBalanceViewModel InventoryBalanceVM { get; }

    // خصائص إدارة الوحدات
    [ObservableProperty] private UnitModel? selectedUnitToAdd;
    [ObservableProperty] private string? unitBarcodeToAdd;
    [ObservableProperty] private ItemUnit? selectedItemUnit;

    // لعرض View داخل تبويب الأسعار
    [ObservableProperty] private object? currentView;
    
    // مؤشرات التحميل
    [ObservableProperty] private bool isLoading = false;
    [ObservableProperty] private string loadingMessage = "جاري التحميل...";
    
    // متغيرات لتجنب التحميل المتكرر
    private bool _lookupsLoaded = false;
    private readonly SemaphoreSlim _loadLookupsSemaphore = new SemaphoreSlim(1, 1);
    
    // تتبع حالة تحميل التبويبات (Lazy Loading)
    private bool _pricesLoaded = false;
    private bool _unitsLoaded = false;
    private bool _costsLoaded = false;
    private bool _suppliersLoaded = false;
    private bool _batchesLoaded = false;
    private bool _componentsLoaded = false;
    private bool _inventoryLoaded = false;

    public ItemEditViewModel(
        IItemsService service,
        IUnitsLookupService units,
        ICategoriesLookupService cats,
        IValuationGroupService valGroups,
        IItemTypeService itemTypeService,
        IItemCostsService itemCostsService,
        IItemPricesService itemPricesService,
        IItemBatchesService itemBatchesService,
        IItemComponentsService itemComponentsService,
        IItemSuppliersService itemSuppliersService,
        ISuppliersService suppliersService,
        NextCurrencyService currencyService,
        IInventoryBalanceService inventoryBalanceService,
        IWarehouseService warehouseService,
        Item model)
    {
        _service = service;
        _units = units;
        _cats = cats;
        _valGroups = valGroups;
        _itemTypeService = itemTypeService;
        _itemCostsService = itemCostsService;
        _itemPricesService = itemPricesService;
        _itemBatchesService = itemBatchesService;
        _itemComponentsService = itemComponentsService;
        _itemSuppliersService = itemSuppliersService;
        _suppliersService = suppliersService;
        _currencyService = currencyService;
        _inventoryBalanceService = inventoryBalanceService;
        _warehouseService = warehouseService;

        // إنشاء ViewModels للتبويبات المتقدمة أولاً
        ItemCostsVM = new ItemCostsViewModel(_itemCostsService);
        ItemPricesVM = new ItemPricesViewModel(_itemPricesService, _units);
        ItemSuppliersVM = new ItemSuppliersViewModel(_itemSuppliersService, _suppliersService, _currencyService);
        ItemBatchesVM = new ItemBatchesViewModel(_itemBatchesService);
        ItemComponentsVM = new ItemComponentsViewModel(_itemComponentsService, _units);
        InventoryBalanceVM = new InventoryBalanceViewModel(_inventoryBalanceService, _warehouseService, _units);
        
        // تعيين Model بعد إنشاء ViewModels لتجنب NullReferenceException
        Model = Clone(model);

        // تحميل البيانات المرجعية وتحديث ViewModels بشكل آمن
        _ = LoadDataAsync();
    }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "جاري تحميل البيانات المرجعية...";
                
                // تحميل البيانات المرجعية بشكل متسلسل
                await LoadLookupsAsync();
                
                // تحديث ViewModels بعد تحميل البيانات المرجعية
                if (Model.ItemID > 0)
                {
                    LoadingMessage = "جاري تحديث البيانات المرتبطة...";
                    await UpdateViewModelsAsync();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = "جاري التحميل...";
            }
        }

        private static Task RunOnUIAsync(Action action)
        {
            var d = Application.Current?.Dispatcher;
            if (d == null || d.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }
            return d.InvokeAsync(action).Task;
        }

        // طرق Lazy Loading للتبويبات
        public async Task LoadPricesTabAsync()
        {
            if (ItemPricesVM == null) return;
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadPricesTabAsync: Setting context for ItemPricesVM with ItemID = {Model.ItemID}");
                await Application.Current.Dispatcher.InvokeAsync(() => 
                    ItemPricesVM.SetContext(isTab: true, itemId: Model.ItemID));
                _pricesLoaded = true;
                System.Diagnostics.Debug.WriteLine($"LoadPricesTabAsync: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في تحميل تبويب الأسعار: {ex.Message}");
                _pricesLoaded = false;
            }
        }

        public async Task LoadUnitsTabAsync()
        {
            if (Model.ItemID <= 0) return;
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadUnitsTabAsync: Loading units for ItemID = {Model.ItemID}");
                _unitsLoaded = true;
                
                // تحميل وحدات الصنف من قاعدة البيانات
                // استخدام ItemsService للحصول على وحدات الصنف
                var itemWithUnits = await _service.GetByIdAsync(Model.ItemID);
                if (itemWithUnits != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ItemUnits.Clear();
                        // إضافة الوحدة الأساسية إذا كانت موجودة
                        if (itemWithUnits.UnitID > 0)
                        {
                            var primaryUnit = AllUnits.FirstOrDefault(u => u.UnitID == itemWithUnits.UnitID);
                            if (primaryUnit != null)
                            {
                                ItemUnits.Add(new ItemUnit
                                {
                                    UnitID = primaryUnit.UnitID,
                                    Unit = primaryUnit,
                                    IsPrimary = true,
                                    IsSalesUnit = true,
                                    PurchaseUnit = true,
                                    IsInventoryUnit = true
                                });
                            }
                        }
                    });
                }
                System.Diagnostics.Debug.WriteLine($"LoadUnitsTabAsync: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في تحميل تبويب الوحدات: {ex.Message}");
                _unitsLoaded = false;
            }
        }

        public async Task LoadCostsTabAsync()
        {
            if (_costsLoaded || ItemCostsVM == null) return;
            
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => 
                    ItemCostsVM.ItemId = Model.ItemID);
                _costsLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في تحميل تبويب التكاليف: {ex.Message}");
                _costsLoaded = false;
            }
        }

        public async Task LoadSuppliersTabAsync()
        {
            if (_suppliersLoaded || ItemSuppliersVM == null) return;
            
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => 
                    ItemSuppliersVM.CurrentItemId = Model.ItemID);
                _suppliersLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في تحميل تبويب الموردين: {ex.Message}");
                _suppliersLoaded = false;
            }
        }

        public async Task LoadBatchesTabAsync()
        {
            if (_batchesLoaded || ItemBatchesVM == null) return;
            
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => 
                    ItemBatchesVM.CurrentItemId = Model.ItemID);
                _batchesLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في تحميل تبويب الدفعات: {ex.Message}");
                _batchesLoaded = false;
            }
        }

        public async Task LoadComponentsTabAsync()
        {
            if (_componentsLoaded || ItemComponentsVM == null) return;
            
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => 
                    ItemComponentsVM.CurrentItemId = Model.ItemID);
                _componentsLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في تحميل تبويب المكونات: {ex.Message}");
                _componentsLoaded = false;
            }
        }

        public async Task LoadInventoryTabAsync()
        {
            if (_inventoryLoaded || InventoryBalanceVM == null) return;
            
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => 
                    InventoryBalanceVM.CurrentItemId = Model.ItemID);
                _inventoryLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في تحميل تبويب الأرصدة: {ex.Message}");
                _inventoryLoaded = false;
            }
        }


        private async Task UpdateViewModelsAsync()
        {
            try
            {
                // التحقق من أن ViewModels تم إنشاؤها
                if (ItemPricesVM == null)
                    return;
                    
                // تحميل التبويب الافتراضي فقط (الأسعار)
                if (Model.ItemID > 0)
                {
                    // تحميل بيانات الأسعار تلقائياً
                    await LoadPricesTabAsync();
                    
                    // تحديث شاشة الأسعار
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        CurrentView = new ItemPricesView
                        {
                            DataContext = ItemPricesVM
                        };
                    });
                    
                    // إعداد ItemId للتبويبات الأخرى (بدون تحميل البيانات)
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (ItemCostsVM != null) ItemCostsVM.ItemId = Model.ItemID;
                        if (ItemSuppliersVM != null) ItemSuppliersVM.CurrentItemId = Model.ItemID;
                        if (ItemBatchesVM != null) ItemBatchesVM.CurrentItemId = Model.ItemID;
                        if (ItemComponentsVM != null) ItemComponentsVM.CurrentItemId = Model.ItemID;
                        if (InventoryBalanceVM != null) InventoryBalanceVM.CurrentItemId = Model.ItemID;
                    });
                }
                else
                {
                    // للصنف الجديد، إعداد ViewModels للوضع الجديد
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ItemPricesVM.SetContext(isTab: true, itemId: null);
                        
                        // مسح وحدات الصنف للصنف الجديد
                        ItemUnits.Clear();
                        
                        // تحديث شاشة الأسعار
                        CurrentView = new ItemPricesView
                        {
                            DataContext = ItemPricesVM
                        };
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في UpdateViewModels: {ex.Message}");
            }
        }

    private async Task LoadLookupsAsync()
    {
        await _loadLookupsSemaphore.WaitAsync();
        try
        {
            if (_lookupsLoaded) return; // تجنب التحميل المتكرر

            // تحميل البيانات المرجعية بشكل متسلسل لتجنب مشاكل التزامن
            var units = await _units.GetAllAsync();
            await Task.Delay(100); // تأخير بين العمليات
            
            var categories = await _cats.GetAllAsync();
            await Task.Delay(100); // تأخير بين العمليات
            
            var valuationGroups = await _valGroups.GetListAsync(companyId: 1);
            await Task.Delay(100); // تأخير بين العمليات
            
            var itemTypes = await _itemTypeService.GetAllAsync();

            // تحديث المجموعات على UI thread
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                AllUnits.Clear();
                foreach (var u in units) AllUnits.Add(u);

                AllCategories.Clear();
                foreach (var c in categories) AllCategories.Add(c);

                AllValuationGroups.Clear();
                foreach (var vg in valuationGroups) AllValuationGroups.Add(vg);

                AllItemTypes.Clear();
                foreach (var it in itemTypes) AllItemTypes.Add(it);
            });

            _lookupsLoaded = true; // تم التحميل بنجاح
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show($"خطأ في تحميل البيانات المرجعية: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        finally
        {
            IsLoading = false;
            _loadLookupsSemaphore.Release();
        }
    }

        private async Task LoadItemUnitsAsync()
        {
            await RunOnUIAsync(() => ItemUnits.Clear());   // كان ItemUnits.Clear() مباشرة

            var id = Model?.ItemID ?? 0;
            if (id <= 0) return;

            try
            {
                // انتظار تحميل البيانات المرجعية أولاً
                await _loadLookupsSemaphore.WaitAsync();
                try
                {
                    if (!_lookupsLoaded)
                        await LoadLookupsAsync();
                }
                finally
                {
                    _loadLookupsSemaphore.Release();
                }

                var rows = await _service.GetItemUnitsAsync(id);

                await RunOnUIAsync(() =>
                {
                    foreach (var r in rows)
                    {
                        if (r.Unit == null && AllUnits?.Count > 0)
                            r.Unit = AllUnits.FirstOrDefault(u => u.UnitID == r.UnitID);

                        ItemUnits.Add(r);     // الإضافة الآن على ثريد الـUI
                    }
                });
            }
            catch (Exception ex)
            {
                await RunOnUIAsync(() =>
                {
                    MessageBox.Show($"خطأ في تحميل وحدات الصنف: {ex.Message}", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        // دالة منفصلة لتحميل وحدات الصنف عند الحاجة
     public async Task LoadItemUnitsIfNeededAsync()
    {
        if (Model?.ItemID > 0 && ItemUnits.Count == 0)
        {
            await LoadItemUnitsAsync();
        }
    }

    // ======= إدارة وحدات الصنف =======
    [RelayCommand]
    public void AddUnit()
    {
        if (Model.ItemID == 0)
        {
            MessageBox.Show("احفظ الصنف أولًا قبل إضافة الوحدات.", "تنبيه");
            return;
        }
        if (SelectedUnitToAdd is null)
        {
            MessageBox.Show("اختر وحدة أولًا.", "تنبيه");
            return;
        }
        if (ItemUnits.Any(x => x.UnitID == SelectedUnitToAdd.UnitID))
        {
            MessageBox.Show("هذه الوحدة مضافة مسبقًا.", "تنبيه");
            return;
        }

        var barcode = (UnitBarcodeToAdd ?? "").Trim();
        if (barcode.Length == 0)
        {
            MessageBox.Show("أدخل رقم الباركود للوحدة.", "تنبيه");
            return;
        }

        ItemUnits.Add(new ItemUnit
        {
            ItemID = Model.ItemID,
            UnitID = SelectedUnitToAdd.UnitID,
            Unit = SelectedUnitToAdd,
            UnitBarcode = barcode,
            IsPrimary = ItemUnits.Count == 0,
            IsSalesUnit = true,
            PurchaseUnit = true,
            IsInventoryUnit = true
        });

        SelectedUnitToAdd = null;
        UnitBarcodeToAdd = null;
    }

    [RelayCommand]
    public void RemoveUnit(ItemUnit? unitToRemove = null)
    {
        var unit = unitToRemove ?? SelectedItemUnit;
        if (unit == null) return;

        bool wasPrimary = unit.IsPrimary.HasValue && unit.IsPrimary.Value;
        ItemUnits.Remove(unit);

        if (wasPrimary && ItemUnits.Count > 0)
        {
            foreach (var r in ItemUnits) r.IsPrimary = false;
            ItemUnits[0].IsPrimary = true;
            Model.UnitID = ItemUnits[0].UnitID;
        }
    }

    [RelayCommand]
    public void MakePrimary(ItemUnit? unitToMakePrimary = null)
    {
        var unit = unitToMakePrimary ?? SelectedItemUnit;
        if (unit == null) return;

        foreach (var r in ItemUnits) r.IsPrimary = false;
        unit.IsPrimary = true;
        Model.UnitID = unit.UnitID;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        try
        {
            // التحقق من صحة البيانات الأساسية
            if (string.IsNullOrWhiteSpace(Model.ItemName))
            {
                MessageBox.Show("اسم الصنف مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Model.CategoryID <= 0)
            {
                MessageBox.Show("الفئة مطلوبة.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.ItemType))
            {
                MessageBox.Show("نوع الصنف مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Model.UnitID <= 0)
            {
                MessageBox.Show("الوحدة الأساسية مطلوبة.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            LoadingMessage = "جاري الحفظ...";

            var isNew = Model.ItemID == 0;
            string? newItemCode = null;

            // حفظ الصنف أولاً
            if (isNew)
            {
                var (newId, generatedCode) = await _service.AddAsync(Clone(Model));
                Model.ItemID = newId;
                newItemCode = generatedCode;
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show("تم إضافة الصنف بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
            else
            {
                newItemCode = await _service.UpdateAsync(Clone(Model));
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show("تم تحديث الصنف بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }

            // حفظ الوحدات بعد حفظ الصنف
            await _service.SetItemUnitsAsync(
                Model.ItemID,
                ItemUnits.Select(x => new ItemUnit
                {
                    ItemID = Model.ItemID,
                    UnitID = x.UnitID,
                    UnitBarcode = x.UnitBarcode,
                    BarcodeType = x.BarcodeType,
                    IsPrimary = x.IsPrimary,
                    IsSalesUnit = x.IsSalesUnit,
                    PurchaseUnit = x.PurchaseUnit,
                    IsInventoryUnit = x.IsInventoryUnit
                })
            );

            // تحديث ViewModels للتبويبات المتقدمة
            await UpdateViewModelsAsync();
            CloseRequested?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand] public void Cancel() => CloseRequested?.Invoke(this, false);

    public event EventHandler<bool>? CloseRequested;

    private static Item Clone(Item s) => new()
    {
        ItemID = s.ItemID,
        ItemCode = s.ItemCode,
        ItemName = s.ItemName,
        CategoryID = s.CategoryID,
        ItemBarcode = s.ItemBarcode,
        ItemType = s.ItemType,
        IsBatchTracked = s.IsBatchTracked,
        IsSerialTracked = s.IsSerialTracked,
        HasExpiryDate = s.HasExpiryDate,
        UnitID = s.UnitID,
        Weight = s.Weight,
        Volume = s.Volume,
        MinStockLevel = s.MinStockLevel,
        MaxStockLevel = s.MaxStockLevel,
        ReorderLevel = s.ReorderLevel,
        StandardCost = s.StandardCost,
        LastPurchasePrice = s.LastPurchasePrice,
        ValuationGroup = s.ValuationGroup,
        IsActive = s.IsActive
    };
    }
}
