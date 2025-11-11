using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Items.Views;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.Items.ViewModels
{
    public partial class ItemsViewModel : ObservableObject
    {
        private readonly IItemsService _service;
        private readonly IUnitsLookupService _units;
        private readonly ICategoriesLookupService _cats;
        private readonly IValuationGroupService _valGroups;
        private readonly IItemTypeService _itemTypeService;

        public ObservableCollection<Item> Items { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private Item? selectedItem;

        [ObservableProperty] private string? searchText;

        private readonly RelayCommand _newDialogCommand;
        private readonly RelayCommand _editDialogCommand;
        private readonly AsyncRelayCommand _deleteCommand;
        private readonly AsyncRelayCommand _refreshCommand;
        private readonly AsyncRelayCommand _loadCommand;

        public RelayCommand NewDialogCommand => _newDialogCommand;
        public RelayCommand EditDialogCommand => _editDialogCommand;
        public AsyncRelayCommand DeleteCommand => _deleteCommand;
        public AsyncRelayCommand RefreshCommand => _refreshCommand;
        public AsyncRelayCommand LoadCommand => _loadCommand;

        public ItemsViewModel(
            IItemsService service,
            IUnitsLookupService units,
            ICategoriesLookupService cats,
            IValuationGroupService valGroups,
            IItemTypeService itemTypeService)
        {
            _service = service;
            _units = units;
            _cats = cats;
            _valGroups = valGroups;
            _itemTypeService = itemTypeService;

            // تهيئة الأوامر
            _newDialogCommand = new RelayCommand(OpenNewDialog);
            _editDialogCommand = new RelayCommand(OpenEditDialog, () => SelectedItem != null);
            _deleteCommand = new AsyncRelayCommand(DeleteAsync);
            _refreshCommand = new AsyncRelayCommand(LoadAsync);
            _loadCommand = new AsyncRelayCommand(LoadAsync);
            
            // تحميل البيانات بشكل آمن
            _ = Task.Run(async () => await LoadAsync());
        }

        partial void OnSelectedItemChanged(Item? oldValue, Item? newValue)
            => EditDialogCommand.NotifyCanExecuteChanged();

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
        {
            // تحميل البيانات بشكل آمن مع تأخير لمنع التحميل المتكرر
            _ = Task.Run(async () => 
            {
                await Task.Delay(300); // تأخير 300ms لمنع التحميل المتكرر أثناء الكتابة
                await LoadAsync();
            });
        }

        public async Task LoadAsync()
        {
            try
            {
                var list = await _service.GetAllAsync(SearchText);
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Items.Clear();
                    foreach (var item in list) Items.Add(item);
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"خطأ في تحميل الأصناف: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void OpenNewDialog()
        {
            try
            {
                // الحصول على الـ services من DI
                var itemCostsService = App.ServiceProvider.GetRequiredService<IItemCostsService>();
                var itemPricesService = App.ServiceProvider.GetRequiredService<IItemPricesService>();
                var itemBatchesService = App.ServiceProvider.GetRequiredService<IItemBatchesService>();
                var itemComponentsService = App.ServiceProvider.GetRequiredService<IItemComponentsService>();
                var itemSuppliersService = App.ServiceProvider.GetRequiredService<IItemSuppliersService>();
                var suppliersService = App.ServiceProvider.GetRequiredService<ISuppliersService>();
                var currencyService = App.ServiceProvider.GetRequiredService<NextCurrencyService>();
                var inventoryBalanceService = App.ServiceProvider.GetRequiredService<IInventoryBalanceService>();
                var warehouseService = App.ServiceProvider.GetRequiredService<IWarehouseService>();
                
                var vm = new ItemEditViewModel(_service, _units, _cats, _valGroups, _itemTypeService, 
                    itemCostsService, itemPricesService, itemBatchesService, itemComponentsService, 
                    itemSuppliersService, suppliersService, currencyService, inventoryBalanceService, warehouseService,
                    new Item { IsActive = true, ItemType = "S" });
                var win = new ItemEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
                if (win.ShowDialog() == true) _ = LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة الإضافة: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            try
            {
                // الحصول على الـ services من DI
                var itemCostsService = App.ServiceProvider.GetRequiredService<IItemCostsService>();
                var itemPricesService = App.ServiceProvider.GetRequiredService<IItemPricesService>();
                var itemBatchesService = App.ServiceProvider.GetRequiredService<IItemBatchesService>();
                var itemComponentsService = App.ServiceProvider.GetRequiredService<IItemComponentsService>();
                var itemSuppliersService = App.ServiceProvider.GetRequiredService<IItemSuppliersService>();
                var suppliersService = App.ServiceProvider.GetRequiredService<ISuppliersService>();
                var currencyService = App.ServiceProvider.GetRequiredService<NextCurrencyService>();
                var inventoryBalanceService = App.ServiceProvider.GetRequiredService<IInventoryBalanceService>();
                var warehouseService = App.ServiceProvider.GetRequiredService<IWarehouseService>();
                
                var vm = new ItemEditViewModel(_service, _units, _cats, _valGroups, _itemTypeService, 
                    itemCostsService, itemPricesService, itemBatchesService, itemComponentsService, 
                    itemSuppliersService, suppliersService, currencyService, inventoryBalanceService, warehouseService,
                    Clone(SelectedItem));
                var win = new ItemEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
                if (win.ShowDialog() == true) _ = LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة التعديل: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            
            try
            {
                var result = await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    return MessageBox.Show("هل تريد حذف الصنف المحدد؟", "تأكيد", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                });

                if (result == MessageBoxResult.Yes)
                {
                    await _service.DeleteAsync(SelectedItem.ItemID);
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"خطأ في حذف الصنف: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private static Item Clone(Item model) => new()
        {
            ItemID = model.ItemID,
            ItemCode = model.ItemCode,
            ItemName = model.ItemName,
            CategoryID = model.CategoryID,
            ItemBarcode = model.ItemBarcode,
            ItemType = model.ItemType,
            IsBatchTracked = model.IsBatchTracked,
            IsSerialTracked = model.IsSerialTracked,
            HasExpiryDate = model.HasExpiryDate,
            UnitID = model.UnitID,
            Weight = model.Weight,
            Volume = model.Volume,
            MinStockLevel = model.MinStockLevel,
            MaxStockLevel = model.MaxStockLevel,
            ReorderLevel = model.ReorderLevel,
            StandardCost = model.StandardCost,
            LastPurchasePrice = model.LastPurchasePrice,
            ValuationGroup = model.ValuationGroup,
            IsActive = model.IsActive
        };

    }
}
