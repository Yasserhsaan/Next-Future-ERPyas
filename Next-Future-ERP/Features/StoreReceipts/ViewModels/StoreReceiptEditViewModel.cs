using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.StoreReceipts.Models;
using Next_Future_ERP.Features.StoreReceipts.Services;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using Next_Future_ERP.Features.Purchases.Services;
using Next_Future_ERP.Features.Purchases.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.StoreReceipts.ViewModels
{
    public partial class StoreReceiptEditViewModel : ObservableObject
    {
        private readonly IStoreReceiptsService _service;
        private readonly ISuppliersService _suppliersService;
        private readonly IItemsService _itemsService;
        private readonly IUnitsService _unitsService;
        private readonly IWarehouseService _warehousesService;
        private readonly IPurchaseTxnsService _purchaseTxnsService;

        public ObservableCollection<Supplier> Suppliers { get; } = new();
        public ObservableCollection<Item> Items { get; } = new();
        public ObservableCollection<UnitModel> Units { get; } = new();
        public ObservableCollection<Warehouse> Warehouses { get; } = new();
        public ObservableCollection<StoreReceiptDetailed> Details { get; } = new();

        [ObservableProperty] private StoreReceipt receipt = new();
        [ObservableProperty] private StoreReceiptDetailed? selectedDetail;
        [ObservableProperty] private bool isEditMode;
        [ObservableProperty] private decimal totalAmount;
        [ObservableProperty] private bool isPurchaseOrderSelected;
        [ObservableProperty] private string selectedPurchaseOrderNumber = string.Empty;
        [ObservableProperty] private string windowTitle = "سند الفحص والاستلام";

        // خاصية لتتبع تغييرات التفاصيل
        private bool _isUpdatingDetails = false;

        // خاصية لتحديث الحسابات عند تغيير قيم التفاصيل
        public void OnDetailPropertyChanged()
        {
            if (!_isUpdatingDetails)
                CalculateTotal();
        }

        public IRelayCommand SaveCommand => new AsyncRelayCommand(SaveAsync);
        public IRelayCommand CancelCommand => new RelayCommand(Cancel);
        public IRelayCommand AddDetailCommand => new RelayCommand(AddDetail);
        public IRelayCommand RemoveDetailCommand => new RelayCommand(RemoveDetail, () => SelectedDetail != null);
        public IRelayCommand LoadFromPurchaseOrderCommand => new RelayCommand(LoadFromPurchaseOrder);

        public StoreReceiptEditViewModel(
            IStoreReceiptsService service,
            ISuppliersService suppliersService,
            IItemsService itemsService,
            IUnitsService unitsService,
            IWarehouseService warehousesService,
            IPurchaseTxnsService purchaseTxnsService,
            StoreReceipt receipt)
        {
            _service = service;
            _suppliersService = suppliersService;
            _itemsService = itemsService;
            _unitsService = unitsService;
            _warehousesService = warehousesService;
            _purchaseTxnsService = purchaseTxnsService;
            
            Receipt = receipt;
            IsEditMode = receipt.ReceiptId > 0;
            
            // تعيين القيم الافتراضية
            if (Receipt.CompanyId == 0) Receipt.CompanyId = 1;
            if (Receipt.BranchId == 0) Receipt.BranchId = 1;
            if (Receipt.CurrencyId == 0) Receipt.CurrencyId = 1;
            if (Receipt.ExchangeRate == 0) Receipt.ExchangeRate = 1;
            if (Receipt.ReceiptDate == default) Receipt.ReceiptDate = DateTime.Today;
            
            // ربط حدث تغيير التفاصيل
            Details.CollectionChanged += (s, e) => 
            {
                if (!_isUpdatingDetails)
                    CalculateTotal();
            };
            
            _ = LoadDataAsync();
        }

        public static async Task<StoreReceiptEditViewModel> FromExisting(
            IStoreReceiptsService service,
            ISuppliersService suppliersService,
            IItemsService itemsService,
            IUnitsService unitsService,
            IWarehouseService warehousesService,
            IPurchaseTxnsService purchaseTxnsService,
            long receiptId)
        {
            var receipt = await service.GetByIdAsync(receiptId);
            if (receipt == null)
                throw new InvalidOperationException("سند الاستلام غير موجود.");

            return new StoreReceiptEditViewModel(service, suppliersService, itemsService, unitsService, warehousesService, purchaseTxnsService, receipt);
        }

        partial void OnSelectedDetailChanged(StoreReceiptDetailed? oldValue, StoreReceiptDetailed? newValue)
            => (RemoveDetailCommand as RelayCommand)?.NotifyCanExecuteChanged();

        partial void OnReceiptChanged(StoreReceipt oldValue, StoreReceipt newValue)
        {
            // منع تعديل المورد يدوياً
            if (IsPurchaseOrderSelected && oldValue.SupplierId != newValue.SupplierId)
            {
                MessageBox.Show("لا يمكن تعديل المورد - يتم ملؤه تلقائياً من أمر الشراء المختار.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                Receipt.SupplierId = oldValue.SupplierId;
                return;
            }

            // تحديث العملة وسعر الصرف في التفاصيل عند تغييرها في الرأس
            foreach (var detail in Details)
            {
                detail.CurrencyId = newValue.CurrencyId;
                detail.ExchangeRate = newValue.ExchangeRate;
            }
            CalculateTotal();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // توليد رقم السند التلقائي إذا كان جديد
                if (!IsEditMode && string.IsNullOrWhiteSpace(Receipt.ReceiptNumber))
                {
                    Receipt.ReceiptNumber = await _service.GenerateNextNumberAsync(Receipt.CompanyId, Receipt.BranchId);
                    // إشعار UI بتحديث الرقم
                    OnPropertyChanged(nameof(Receipt));
                }

                // تحميل الموردين
                var suppliers = await _suppliersService.GetAllAsync();
                foreach (var supplier in suppliers.OrderBy(x => x.SupplierName))
                    Suppliers.Add(supplier);

                // تحميل الأصناف
                var items = await _itemsService.GetAllAsync();
                foreach (var item in items.OrderBy(x => x.ItemName))
                    Items.Add(item);

                // تحميل الوحدات
                var units = await _unitsService.GetAllAsync();
                foreach (var unit in units.OrderBy(x => x.UnitName))
                    Units.Add(unit);

                // تحميل المستودعات
                var warehouses = await _warehousesService.GetAllAsync();
                foreach (var warehouse in warehouses.OrderBy(x => x.WarehouseName))
                    Warehouses.Add(warehouse);

                // تحميل التفاصيل إذا كان في وضع التعديل
                if (IsEditMode && Receipt.Details.Any())
                {
                    _isUpdatingDetails = true;
                    foreach (var detail in Receipt.Details)
                        Details.Add(detail);
                    _isUpdatingDetails = false;
                }

                CalculateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddDetail()
        {
            // منع الإدخال اليدوي بدون أمر شراء
            if (!IsPurchaseOrderSelected)
            {
                MessageBox.Show("يجب اختيار أمر شراء أولاً قبل إضافة تفاصيل.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var detail = new StoreReceiptDetailed
            {
                ReceiptId = Receipt.ReceiptId,
                ItemId = 0,
                UnitId = 0,
                Quantity = 1,
                UnitPrice = 0,
                WarehouseId = 0,
                CurrencyId = Receipt.CurrencyId,
                ExchangeRate = Receipt.ExchangeRate,
                VatRate = 0
            };

            Details.Add(detail);
            // CalculateTotal() سيتم استدعاؤه تلقائياً من خلال CollectionChanged
        }

        private void RemoveDetail()
        {
            if (SelectedDetail != null)
            {
                Details.Remove(SelectedDetail);
                // CalculateTotal() سيتم استدعاؤه تلقائياً من خلال CollectionChanged
            }
        }

        private void CalculateTotal()
        {
            // إعادة حساب المجاميع لكل سطر
            foreach (var detail in Details)
            {
                CalculateDetailTotals(detail);
            }
            
            TotalAmount = Details.Sum(d => d.TotalPrice ?? 0);
        }

        private void CalculateDetailTotals(StoreReceiptDetailed detail)
        {
            detail.SubTotal = Math.Round(detail.Quantity * detail.UnitPrice, 4);
            
            if (detail.VatRate.HasValue && detail.VatRate > 0)
            {
                detail.VatAmount = Math.Round(detail.SubTotal.Value * (detail.VatRate.Value / 100m), 4);
            }
            else
            {
                detail.VatAmount = 0;
            }

            detail.TotalPrice = Math.Round(detail.SubTotal.Value + detail.VatAmount.Value, 4);
        }

        private async Task SaveAsync()
        {
            try
            {
                // التحقق من وجود أمر شراء
                if (!IsPurchaseOrderSelected || Receipt.PurchaseOrderId == null || Receipt.PurchaseOrderId == 0)
                {
                    MessageBox.Show("يجب اختيار أمر شراء أولاً قبل الحفظ.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateData())
                    return;

                // تعيين القيم المطلوبة
                if (!IsEditMode)
                {
                    Receipt.CreatedBy = 1; // TODO: استخدام المستخدم الحالي
                    Receipt.Status = 0; // مسودة
                }
                else
                {
                    Receipt.ModifiedBy = 1; // TODO: استخدام المستخدم الحالي
                    Receipt.ModifiedAt = DateTime.Now;
                }

                if (IsEditMode)
                {
                    await _service.UpdateAsync(Receipt, Details);
                    MessageBox.Show("تم تحديث سند الاستلام بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();
                }
                else
                {
                    // تحديث أمر الشراء أولاً للتأكد من صحة البيانات
                    await UpdatePurchaseOrderReceivedQuantities();
                    
                    // ثم حفظ سند الاستلام
                    var id = await _service.AddAsync(Receipt, Details);
                    Receipt.ReceiptId = id;
                    
                    IsEditMode = true;
                    MessageBox.Show("تم إنشاء سند الاستلام وتحديث أمر الشراء بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في الحفظ: {ex.Message}\n\nتفاصيل الخطأ: {ex}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateData()
        {
            if (Receipt.SupplierId == null || Receipt.SupplierId <= 0)
            {
                MessageBox.Show("يرجى اختيار المورد.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Receipt.ReceiptDate == default)
            {
                MessageBox.Show("يرجى تحديد تاريخ السند.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Details.Any())
            {
                MessageBox.Show("يرجى إضافة تفاصيل للسند.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            foreach (var detail in Details)
            {
                if (detail.ItemId <= 0)
                {
                    MessageBox.Show("يرجى اختيار الصنف لكل سطر.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (detail.UnitId <= 0)
                {
                    MessageBox.Show("يرجى اختيار الوحدة لكل سطر.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (detail.WarehouseId <= 0)
                {
                    MessageBox.Show("يرجى اختيار المستودع لكل سطر.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (detail.Quantity <= 0)
                {
                    MessageBox.Show("الكمية يجب أن تكون أكبر من صفر.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // التحقق من أن الكمية لا تتجاوز الكمية المتبقية
                if (detail.Quantity > detail.RemainingQuantity)
                {
                    MessageBox.Show($"الكمية المدخلة ({detail.Quantity}) تتجاوز الكمية المتبقية من أمر الشراء ({detail.RemainingQuantity}).", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (detail.UnitPrice < 0)
                {
                    MessageBox.Show("سعر الوحدة لا يقبل السالب.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void Cancel()
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            // إغلاق النافذة
            if (Application.Current.Windows.OfType<Views.StoreReceiptEditWindow>().FirstOrDefault() is { } window)
                window.Close();
        }

        private void LoadFromPurchaseOrder()
        {
            try
            {
                var selectionWindow = new Views.PurchaseOrderSelectionWindow();
                if (selectionWindow.ShowDialog() == true && selectionWindow.SelectedPurchaseOrder != null)
                {
                    var selectedPO = selectionWindow.SelectedPurchaseOrder;
                    
                    // تحديث بيانات الرأس
                    Receipt.PurchaseOrderId = selectedPO.TxnID;
                    Receipt.SupplierId = selectedPO.SupplierID;
                    Receipt.Description = $"استلام من أمر الشراء {selectedPO.TxnNumber}";
                    
                    // تحديث خصائص الواجهة
                    IsPurchaseOrderSelected = true;
                    SelectedPurchaseOrderNumber = selectedPO.TxnNumber;
                    
                    // تحديث اسم المورد في الواجهة
                    OnPropertyChanged(nameof(Receipt.SupplierId));
                    
                    // تحميل تفاصيل أمر الشراء
                    LoadPurchaseOrderDetails(selectedPO.TxnID);
                    
                    MessageBox.Show($"تم تحميل أمر الشراء {selectedPO.TxnNumber} بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل أمر الشراء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadPurchaseOrderDetails(int purchaseOrderId)
        {
            try
            {
                // تحميل أمر الشراء مع التفاصيل
                var purchaseOrder = await _purchaseTxnsService.GetByIdAsync(purchaseOrderId);
                if (purchaseOrder?.Details != null)
                {
                    // مسح التفاصيل الحالية
                    Details.Clear();
                    
                    // إضافة تفاصيل أمر الشراء
                    foreach (var poDetail in purchaseOrder.Details)
                    {
                        var remainingQty = poDetail.Quantity - (poDetail.ReceivedQuantity ?? 0);
                        
                        var detail = new StoreReceiptDetailed
                        {
                            ItemId = poDetail.ItemID,
                            UnitId = poDetail.UnitID,
                            Quantity = remainingQty, // الكمية المتبقية
                            RemainingQuantity = remainingQty, // للعرض
                            UnitPrice = poDetail.UnitPrice,
                            VatRate = poDetail.VATRate,
                            WarehouseId = 1, // افتراضي - يمكن تحسينه لاحقاً
                            CurrencyId = Receipt.CurrencyId,
                            ExchangeRate = Receipt.ExchangeRate
                        };
                        
                        // حساب المجاميع
                        CalculateDetailTotals(detail);
                        Details.Add(detail);
                    }
                    
                    // حساب المجموع الإجمالي
                    CalculateTotal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل تفاصيل أمر الشراء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdatePurchaseOrderReceivedQuantities()
        {
            if (Receipt.PurchaseOrderId == null || Receipt.PurchaseOrderId <= 0)
                return;

            // تحميل أمر الشراء مع التفاصيل
            var purchaseOrder = await _purchaseTxnsService.GetByIdAsync(Receipt.PurchaseOrderId.Value);
            if (purchaseOrder?.Details == null)
                throw new InvalidOperationException("لم يتم العثور على أمر الشراء المحدد أو تفاصيله");

            // تحديث الكميات المستلمة لكل سطر
            foreach (var poDetail in purchaseOrder.Details)
            {
                var receiptDetail = Details.FirstOrDefault(d => d.ItemId == poDetail.ItemID);
                if (receiptDetail != null)
                {
                    // إضافة الكمية المستلمة إلى الكمية المستلمة السابقة
                    poDetail.ReceivedQuantity = (poDetail.ReceivedQuantity ?? 0) + receiptDetail.Quantity;
                    
                    // تحديد ما إذا كان السطر مغلق أم لا
                    poDetail.IsClosed = poDetail.ReceivedQuantity >= poDetail.Quantity;
                }
            }

            // حفظ التغييرات
            await _purchaseTxnsService.UpdateAsync(purchaseOrder, purchaseOrder.Details);
        }
    }
}
