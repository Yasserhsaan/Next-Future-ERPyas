using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.PurchaseInvoices.Models;
using Next_Future_ERP.Features.PurchaseInvoices.Services;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using Next_Future_ERP.Features.StoreReceipts.Services;
using Next_Future_ERP.Features.Purchases.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.PurchaseInvoices.ViewModels
{
    public partial class PurchaseAPEditViewModel : ObservableObject
    {
        private readonly IPurchaseAPService _service;
        private readonly ISuppliersService _suppliersService;
        private readonly IItemsService _itemsService;
        private readonly IUnitsService _unitsService;
        private readonly IWarehouseService _warehousesService;
        private readonly IStoreReceiptsService _storeReceiptsService;
        private readonly IPurchaseTxnsService _purchaseTxnsService;

        public ObservableCollection<Supplier> Suppliers { get; } = new();
        public ObservableCollection<Item> Items { get; } = new();
        public ObservableCollection<UnitModel> Units { get; } = new();
        public ObservableCollection<Warehouse> Warehouses { get; } = new();
        public ObservableCollection<PurchaseAPDetail> Details { get; } = new();

        [ObservableProperty] private PurchaseAP purchaseAP = new();
        [ObservableProperty] private PurchaseAPDetail? selectedDetail;
        [ObservableProperty] private bool isEditMode;
        [ObservableProperty] private decimal totalAmount;
        [ObservableProperty] private bool isStoreReceiptSelected;
        [ObservableProperty] private string selectedStoreReceiptNumber = string.Empty;
        [ObservableProperty] private string windowTitle = "فاتورة المشتريات/المرتجع";

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
        public IRelayCommand LoadFromStoreReceiptCommand => new RelayCommand(LoadFromStoreReceipt);

        public PurchaseAPEditViewModel(
            IPurchaseAPService service,
            ISuppliersService suppliersService,
            IItemsService itemsService,
            IUnitsService unitsService,
            IWarehouseService warehousesService,
            IStoreReceiptsService storeReceiptsService,
            IPurchaseTxnsService purchaseTxnsService,
            PurchaseAP purchaseAP)
        {
            _service = service;
            _suppliersService = suppliersService;
            _itemsService = itemsService;
            _unitsService = unitsService;
            _warehousesService = warehousesService;
            _storeReceiptsService = storeReceiptsService;
            _purchaseTxnsService = purchaseTxnsService;
            
            PurchaseAP = purchaseAP;
            IsEditMode = purchaseAP.APId > 0;
            
            // تصحيح لتتبع حالة سند الاستلام
            System.Diagnostics.Debug.WriteLine($"PurchaseAPEditViewModel created - IsStoreReceiptSelected: {IsStoreReceiptSelected}");
            
            // تعيين القيم الافتراضية
            if (PurchaseAP.CompanyId == 0) PurchaseAP.CompanyId = 1;
            if (PurchaseAP.BranchId == 0) PurchaseAP.BranchId = 1;
            if (PurchaseAP.CurrencyId == 0) PurchaseAP.CurrencyId = 1;
            if (PurchaseAP.ExchangeRate == 0) PurchaseAP.ExchangeRate = 1;
            if (PurchaseAP.DocDate == default) PurchaseAP.DocDate = DateTime.Today;
            
            // ربط حدث تغيير التفاصيل
            Details.CollectionChanged += (s, e) => 
            {
                if (!_isUpdatingDetails)
                    CalculateTotal();
            };
            
            _ = LoadDataAsync();
        }

        public static async Task<PurchaseAPEditViewModel> FromExisting(
            IPurchaseAPService service,
            ISuppliersService suppliersService,
            IItemsService itemsService,
            IUnitsService unitsService,
            IWarehouseService warehousesService,
            IStoreReceiptsService storeReceiptsService,
            IPurchaseTxnsService purchaseTxnsService,
            long apId)
        {
            var purchaseAP = await service.GetByIdAsync(apId);
            if (purchaseAP == null)
                throw new InvalidOperationException("المستند غير موجود.");

            return new PurchaseAPEditViewModel(service, suppliersService, itemsService, unitsService, 
                warehousesService, storeReceiptsService, purchaseTxnsService, purchaseAP);
        }

        partial void OnSelectedDetailChanged(PurchaseAPDetail? oldValue, PurchaseAPDetail? newValue)
        {
            // لا حاجة لإشعار تغيير الأمر لأننا لا نستخدم RemoveDetailCommand
        }

        partial void OnPurchaseAPChanged(PurchaseAP oldValue, PurchaseAP newValue)
        {
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
                // توليد رقم المستند التلقائي إذا كان جديد
                if (!IsEditMode && string.IsNullOrWhiteSpace(PurchaseAP.DocNumber))
                {
                    PurchaseAP.DocNumber = await _service.GenerateNextNumberAsync(PurchaseAP.CompanyId, PurchaseAP.BranchId, PurchaseAP.DocType);
                    // إشعار UI بتحديث الرقم
                    OnPropertyChanged(nameof(PurchaseAP));
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
                if (IsEditMode && PurchaseAP.Details.Any())
                {
                    _isUpdatingDetails = true;
                    foreach (var detail in PurchaseAP.Details)
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


        private void CalculateTotal()
        {
            // إعادة حساب المجاميع لكل سطر
            foreach (var detail in Details)
            {
                CalculateDetailTotals(detail);
            }
            
            TotalAmount = Details.Sum(d => d.LineTotal ?? 0);
        }

        private void CalculateDetailTotals(PurchaseAPDetail detail)
        {
            var baseAmount = detail.Quantity * detail.UnitPrice * detail.ExchangeRate;
            
            if (detail.PriceIncludesTax && detail.VATRate.HasValue && detail.VATRate > 0)
            {
                // السعر شامل الضريبة
                var rate = detail.VATRate.Value / 100m;
                detail.TaxableAmount = Math.Round(baseAmount / (1 + rate), 4);
                detail.VATAmount = Math.Round(baseAmount - detail.TaxableAmount.Value, 2);
            }
            else
            {
                // السعر غير شامل الضريبة
                detail.TaxableAmount = Math.Round(baseAmount, 4);
                if (detail.VATRate.HasValue && detail.VATRate > 0)
                {
                    detail.VATAmount = Math.Round(detail.TaxableAmount.Value * (detail.VATRate.Value / 100m), 2);
                }
                else
                {
                    detail.VATAmount = 0;
                }
            }
            
            detail.LineTotal = Math.Round(detail.TaxableAmount.Value + detail.VATAmount.Value, 4);
        }

        private async Task SaveAsync()
        {
            try
            {
                // التحقق من وجود سند استلام
                if (!IsStoreReceiptSelected || PurchaseAP.RelatedReceiptId == null || PurchaseAP.RelatedReceiptId == 0)
                {
                    MessageBox.Show("يجب اختيار سند استلام أولاً قبل الحفظ.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateData())
                    return;

                // التحقق المحسن من البيانات
                var validationErrors = await _service.GetValidationErrorsAsync(PurchaseAP, Details);
                if (validationErrors.Any())
                {
                    var errorMessage = "يوجد أخطاء في البيانات:\n" + string.Join("\n", validationErrors);
                    MessageBox.Show(errorMessage, "أخطاء في البيانات", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // تعيين القيم المطلوبة
                if (!IsEditMode)
                {
                    PurchaseAP.CreatedBy = 1; // TODO: استخدام المستخدم الحالي
                    PurchaseAP.Status = 1; // محفوظ
                }
                else
                {
                    PurchaseAP.ModifiedBy = 1; // TODO: استخدام المستخدم الحالي
                    PurchaseAP.ModifiedAt = DateTime.UtcNow;
                }

                // تصحيح مؤقت: حفظ البيانات بدون قاعدة البيانات
                System.Diagnostics.Debug.WriteLine($"Saving PurchaseAP: {PurchaseAP.DocNumber}, Details count: {Details.Count}");
                
                if (IsEditMode)
                {
                    await _service.UpdateAsync(PurchaseAP, Details);
                    MessageBox.Show("تم تحديث المستند بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();
                }
                else
                {
                    var id = await _service.AddAsync(PurchaseAP, Details);
                    PurchaseAP.APId = id;
                    IsEditMode = true;
                    MessageBox.Show("تم إنشاء المستند بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                
                // عرض رسالة خطأ مفصلة
                var errorMessage = $"خطأ في الحفظ: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nتفاصيل الخطأ: {ex.InnerException.Message}";
                }
                
                MessageBox.Show(errorMessage, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateData()
        {
            if (PurchaseAP.SupplierId <= 0)
            {
                MessageBox.Show("يرجى اختيار المورد.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (PurchaseAP.DocDate == default)
            {
                MessageBox.Show("يرجى تحديد تاريخ المستند.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Details.Any())
            {
                MessageBox.Show("يرجى إضافة تفاصيل للمستند.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                if (detail.Quantity <= 0)
                {
                    MessageBox.Show("الكمية يجب أن تكون أكبر من صفر.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            if (Application.Current.Windows.OfType<Views.PurchaseAPEditWindow>().FirstOrDefault() is { } window)
                window.Close();
        }

        private void LoadFromStoreReceipt()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadFromStoreReceipt called - opening selection window...");
                var selectionWindow = new Views.StoreReceiptSelectionWindow();
                if (selectionWindow.ShowDialog() == true && selectionWindow.SelectedStoreReceipt != null)
                {
                    var selectedReceipt = selectionWindow.SelectedStoreReceipt;
                    
                    System.Diagnostics.Debug.WriteLine($"Store receipt selected: {selectedReceipt.ReceiptNumber}, ReceiptId: {selectedReceipt.ReceiptId}");
                    
                    // تحديث بيانات الرأس
                    PurchaseAP.RelatedReceiptId = selectedReceipt.ReceiptId;
                    PurchaseAP.SupplierId = selectedReceipt.SupplierId ?? 0;
                    PurchaseAP.Remarks = $"فاتورة من سند الاستلام {selectedReceipt.ReceiptNumber}";
                    
                    // تحديث خصائص الواجهة
                    IsStoreReceiptSelected = true;
                    SelectedStoreReceiptNumber = selectedReceipt.ReceiptNumber;
                    
                    System.Diagnostics.Debug.WriteLine($"Store receipt selected - IsStoreReceiptSelected: {IsStoreReceiptSelected}, Number: {SelectedStoreReceiptNumber}");
                    
                    // تحديث اسم المورد في الواجهة
                    OnPropertyChanged(nameof(PurchaseAP.SupplierId));
                    
                    // تحميل تفاصيل سند الاستلام
                    System.Diagnostics.Debug.WriteLine($"Calling LoadStoreReceiptDetails with ReceiptId: {selectedReceipt.ReceiptId}");
                    LoadStoreReceiptDetails(selectedReceipt.ReceiptId);
                    
                    MessageBox.Show($"تم تحميل سند الاستلام {selectedReceipt.ReceiptNumber} بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No store receipt selected or dialog cancelled");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadFromStoreReceipt: {ex.Message}");
                MessageBox.Show($"خطأ في تحميل سند الاستلام: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadStoreReceiptDetails(long receiptId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadStoreReceiptDetails called with receiptId: {receiptId}");
                
                // تحميل سند الاستلام مع التفاصيل
                var storeReceipt = await _storeReceiptsService.GetByIdAsync(receiptId);
                System.Diagnostics.Debug.WriteLine($"Store receipt loaded: {storeReceipt?.ReceiptNumber}, Details count: {storeReceipt?.Details?.Count}");
                
                // تصحيح إضافي لرؤية التفاصيل
                if (storeReceipt?.Details != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Details collection type: {storeReceipt.Details.GetType().Name}");
                    foreach (var detail in storeReceipt.Details)
                    {
                        System.Diagnostics.Debug.WriteLine($"Detail: DetailId={detail.DetailId}, ItemId={detail.ItemId}, Quantity={detail.Quantity}, UnitPrice={detail.UnitPrice}");
                    }
                }
                
                if (storeReceipt?.Details != null && storeReceipt.Details.Any())
                {
                    // مسح التفاصيل الحالية
                    Details.Clear();
                    System.Diagnostics.Debug.WriteLine($"Details cleared, current count: {Details.Count}");
                    
                    // إضافة تفاصيل سند الاستلام
                    int lineNo = 1;
                    foreach (var receiptDetail in storeReceipt.Details)
                    {
                        var detail = new PurchaseAPDetail
                        {
                            LineNo = lineNo++,
                            ItemId = receiptDetail.ItemId,
                            UnitId = receiptDetail.UnitId,
                            Quantity = receiptDetail.Quantity,
                            UnitPrice = receiptDetail.UnitPrice,
                            VATRate = receiptDetail.VatRate,
                            WarehouseId = receiptDetail.WarehouseId,
                            CurrencyId = PurchaseAP.CurrencyId,
                            ExchangeRate = PurchaseAP.ExchangeRate,
                            ReceiptDetailId = receiptDetail.DetailId,
                            PriceIncludesTax = PurchaseAP.PriceIncludesTax
                        };
                        
                        System.Diagnostics.Debug.WriteLine($"Adding detail: LineNo={detail.LineNo}, ItemId={detail.ItemId}, Quantity={detail.Quantity}, UnitPrice={detail.UnitPrice}");
                        
                        // حساب المجاميع
                        CalculateDetailTotals(detail);
                        Details.Add(detail);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Final details count: {Details.Count}");
                    
                    // حساب المجموع الإجمالي
                    CalculateTotal();
                    
                    // إشعار الواجهة بالتحديث
                    OnPropertyChanged(nameof(Details));
                    
                    // تصحيح إضافي لتحديث الواجهة
                    System.Diagnostics.Debug.WriteLine($"OnPropertyChanged called for Details, count: {Details.Count}");
                    
                    // إشعار إضافي للواجهة
                    OnPropertyChanged(nameof(TotalAmount));
                    
                    // تحديث DataGrid يدوياً إذا كان متاحاً
                    UpdateDataGrid();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No details found in store receipt");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadStoreReceiptDetails: {ex.Message}");
                MessageBox.Show($"خطأ في تحميل تفاصيل سند الاستلام: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDataGrid()
        {
            try
            {
                // البحث عن DataGrid في الواجهة وتحديثه
                var window = System.Windows.Application.Current.Windows.OfType<Views.PurchaseAPEditWindow>().FirstOrDefault();
                if (window != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (window.DetailsGrid != null)
                        {
                            window.DetailsGrid.ItemsSource = null;
                            window.DetailsGrid.ItemsSource = Details;
                            System.Diagnostics.Debug.WriteLine($"DataGrid updated manually: {Details.Count} items");
                            
                            // تصحيح إضافي لرؤية البيانات
                            if (Details.Count > 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"DataGrid first item: LineNo={Details[0].LineNo}, ItemId={Details[0].ItemId}");
                            }
                            
                            // استدعاء RefreshDataGrid
                            window.RefreshDataGrid();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("DetailsGrid is null");
                        }
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("PurchaseAPEditWindow not found");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating DataGrid: {ex.Message}");
            }
        }
    }
}
