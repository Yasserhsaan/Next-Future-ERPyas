using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Inventory.Models;
using Next_Future_ERP.Features.Inventory.Services;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Inventory.ViewModels
{
    /// <summary>
    /// ViewModel لإدارة شاشة الرصيد الافتتاحي
    /// </summary>
    public partial class InventoryOpeningViewModel : ObservableObject
    {
        private readonly IInventoryOpeningService _inventoryService;
        private readonly IItemsService _itemsService;
        private readonly IWarehouseService _warehouseService;
        private readonly IUnitsService _unitsService;

        #region Properties

        [ObservableProperty]
        private InventoryOpeningHeader currentHeader;

        [ObservableProperty]
        private ObservableCollection<InventoryOpeningDetail> details;

        [ObservableProperty]
        private InventoryOpeningDetail? selectedDetail;

        [ObservableProperty]
        private ObservableCollection<Item> availableItems;

        [ObservableProperty]
        private ObservableCollection<Warehouse> availableWarehouses;

        [ObservableProperty]
        private ObservableCollection<UnitModel> availableUnits;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isEditing;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool canEdit;

        [ObservableProperty]
        private bool canApprove;

        // خصائص الفلترة للإدخال التلقائي
        [ObservableProperty]
        private int[]? selectedCategoryIds;

        [ObservableProperty]
        private int[]? selectedWarehouseIds;

        [ObservableProperty]
        private bool activeItemsOnly = true;

        // خصائص إضافة سطر جديد
        [ObservableProperty]
        private Item? newLineItem;

        [ObservableProperty]
        private Warehouse? newLineWarehouse;

        [ObservableProperty]
        private UnitModel? newLineUnit;

        [ObservableProperty]
        private UnitModel? newLineNumericUnit;

        [ObservableProperty]
        private decimal newLineQty;

        [ObservableProperty]
        private decimal? newLineNumericQty;

        [ObservableProperty]
        private decimal? newLineUnitCost;

        [ObservableProperty]
        private DateTime? newLineExpiryDate;

        [ObservableProperty]
        private string? newLineBatchNo;

        [ObservableProperty]
        private string? newLineSerialNo;

        [ObservableProperty]
        private string? newLineNotes;

        /// <summary>
        /// للتحقق من أن البيانات تم تحميلها
        /// </summary>
        [ObservableProperty]
        private bool isDataLoaded;

        #endregion

        #region Constructor

        public InventoryOpeningViewModel(
            IInventoryOpeningService inventoryService,
            IItemsService itemsService,
            IWarehouseService warehouseService,
            IUnitsService unitsService)
        {
            _inventoryService = inventoryService;
            _itemsService = itemsService;
            _warehouseService = warehouseService;
            _unitsService = unitsService;
            
            CurrentHeader = new InventoryOpeningHeader();
            Details = new ObservableCollection<InventoryOpeningDetail>();
            AvailableItems = new ObservableCollection<Item>();
            AvailableWarehouses = new ObservableCollection<Warehouse>();
            AvailableUnits = new ObservableCollection<UnitModel>();

            // الاشتراك في تغييرات الخصائص
            PropertyChanged += OnPropertyChanged;
            
            // تحميل البيانات عند إنشاء ViewModel
            _ = InitializeAsync();
        }

        /// <summary>
        /// تهيئة البيانات الأولية
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل البيانات...";
                
                // إنشاء مستند جديد افتراضي
                CurrentHeader = new InventoryOpeningHeader
                {
                    DocDate = DateTime.Today,
                    CompanyId = 1, // TODO: الحصول من الجلسة الحالية
                    BranchId = 1,  // TODO: الحصول من الجلسة الحالية
                    CreatedBy = 1  // TODO: الحصول من المستخدم الحالي
                };
                
                // تحميل البيانات المرجعية
                await LoadLookupDataAsync();
                
                StatusMessage = "جاهز للإدخال";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحميل البيانات: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Event Handlers

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CurrentHeader):
                    UpdateCanEditAndApprove();
                    break;
                case nameof(SelectedDetail):
                    LoadDetailForEditing();
                    break;
                case nameof(NewLineItem):
                    OnNewLineItemChanged();
                    break;
            }
        }

        private void UpdateCanEditAndApprove()
        {
            CanEdit = CurrentHeader?.CanEdit ?? false;
            CanApprove = CurrentHeader?.CanApprove ?? false;
        }

        private void LoadDetailForEditing()
        {
            if (SelectedDetail == null) return;

            NewLineItem = SelectedDetail.Item;
            NewLineWarehouse = SelectedDetail.Warehouse;
            NewLineUnit = SelectedDetail.Unit;
            NewLineNumericUnit = SelectedDetail.NumericUnit;
            NewLineQty = SelectedDetail.Qty;
            NewLineNumericQty = SelectedDetail.NumericQty;
            NewLineUnitCost = SelectedDetail.InitialUnitCost;
            NewLineExpiryDate = SelectedDetail.ExpiryDate;
            NewLineBatchNo = SelectedDetail.BatchNo;
            NewLineSerialNo = SelectedDetail.SerialNo;
            NewLineNotes = SelectedDetail.LineNotes;

            IsEditing = true;
        }

        private async void OnNewLineItemChanged()
        {
            if (NewLineItem != null)
            {
                // تحديد الوحدة الأساسية تلقائياً
                NewLineUnit = NewLineItem.Unit;
                
                // تحديد التكلفة المعيارية
                NewLineUnitCost = NewLineItem.StandardCost;
                
                // تحديث قائمة الوحدات المتاحة للصنف
                await LoadItemUnitsAsync(NewLineItem.ItemID);
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task NewDocumentAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري إنشاء مستند جديد...";
                
                CurrentHeader = new InventoryOpeningHeader
                {
                    DocDate = DateTime.Today,
                    CompanyId = 1, // TODO: الحصول من الجلسة الحالية
                    BranchId = 1,  // TODO: الحصول من الجلسة الحالية
                    CreatedBy = 1  // TODO: الحصول من المستخدم الحالي
                };
                
                Details.Clear();
                ClearNewLineData();
                
                // تأكد من تحميل البيانات المرجعية
                if (!AvailableItems.Any() || !AvailableWarehouses.Any() || !AvailableUnits.Any())
                {
                    StatusMessage = "جاري تحميل البيانات المرجعية...";
                    await LoadLookupDataAsync();
                }
                
                StatusMessage = "مستند جديد جاهز للإدخال";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في إنشاء مستند جديد: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"NewDocumentAsync Error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveHeaderAsync()
        {
            try
            {
                IsLoading = true;
                
                if (CurrentHeader.DocID == 0)
                {
                    CurrentHeader = await _inventoryService.CreateHeaderAsync(CurrentHeader);
                    StatusMessage = $"تم إنشاء المستند رقم {CurrentHeader.DocNo}";
                }
                else
                {
                    CurrentHeader = await _inventoryService.UpdateHeaderAsync(CurrentHeader);
                    StatusMessage = "تم حفظ التعديلات";
                }
                
                UpdateCanEditAndApprove();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في الحفظ: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadDocumentAsync(int docId)
        {
            try
            {
                IsLoading = true;
                
                var header = await _inventoryService.GetHeaderByIdAsync(docId);
                if (header == null)
                {
                    StatusMessage = "المستند غير موجود";
                    return;
                }
                
                CurrentHeader = header;
                Details.Clear();
                
                foreach (var detail in header.Details)
                {
                    // ربط المراجع لعرض الأسماء في الجدول
                    detail.Item = AvailableItems.FirstOrDefault(i => i.ItemID == detail.ItemID) ?? detail.Item;
                    detail.Warehouse = AvailableWarehouses.FirstOrDefault(w => w.WarehouseID == detail.WarehouseId) ?? detail.Warehouse;
                    detail.Unit = AvailableUnits.FirstOrDefault(u => u.UnitID == detail.UnitID) ?? detail.Unit;
                    Details.Add(detail);
                }
                
                await LoadLookupDataAsync();
                ClearNewLineData();
                
                StatusMessage = $"تم تحميل المستند رقم {header.DocNo}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحميل المستند: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // أمر مساعد لقبول نص من الواجهة وتحويله إلى رقم
        [RelayCommand]
        private async Task OpenDocumentAsync(object? docIdText)
        {
            if (docIdText == null) return;
            if (int.TryParse(docIdText.ToString(), out var id))
            {
                await LoadDocumentAsync(id);
            }
            else
            {
                StatusMessage = "رقم المستند غير صحيح";
            }
        }

        [RelayCommand]
        private async Task ApproveDocumentAsync()
        {
            try
            {
                if (CurrentHeader.DocID == 0)
                {
                    StatusMessage = "يجب حفظ المستند أولاً";
                    return;
                }
                
                var result = MessageBox.Show(
                    "هل أنت متأكد من اعتماد هذا المستند؟ لن يمكن تعديله بعد الاعتماد.",
                    "تأكيد الاعتماد",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result != MessageBoxResult.Yes) return;
                
                IsLoading = true;
                
                var success = await _inventoryService.ApproveDocumentAsync(CurrentHeader.DocID, 1); // TODO: معرف المستخدم
                
                if (success)
                {
                    CurrentHeader.Status = InventoryOpeningStatus.Approved;
                    CurrentHeader.ApprovedAt = DateTime.Now;
                    CurrentHeader.ApprovedBy = 1; // TODO: معرف المستخدم
                    
                    UpdateCanEditAndApprove();
                    StatusMessage = "تم اعتماد المستند بنجاح";
                }
                else
                {
                    StatusMessage = "فشل في اعتماد المستند";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في الاعتماد: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GenerateAutoDetailsAsync()
        {
            try
            {
                if (CurrentHeader.DocID == 0)
                {
                    StatusMessage = "يجب حفظ المستند أولاً";
                    return;
                }
                
                if (!CanEdit)
                {
                    StatusMessage = "لا يمكن تعديل مستند معتمد";
                    return;
                }
                
                var result = MessageBox.Show(
                    "سيتم توليد أسطر تلقائية بناءً على الفلاتر المحددة. هل تريد المتابعة؟",
                    "توليد تلقائي",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result != MessageBoxResult.Yes) return;
                
                IsLoading = true;
                
                var autoDetails = await _inventoryService.GenerateAutoDetailsAsync(
                    CurrentHeader.DocID, 
                    SelectedCategoryIds, 
                    SelectedWarehouseIds, 
                    ActiveItemsOnly);
                
                foreach (var detail in autoDetails)
                {
                    Details.Add(detail);
                }
                
                StatusMessage = $"تم توليد {autoDetails.Count()} سطر تلقائياً";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في التوليد التلقائي: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddDetailAsync()
        {
            try
            {
                if (!ValidateNewLineData()) return;
                
                if (CurrentHeader.DocID == 0)
                {
                    StatusMessage = "يجب حفظ المستند أولاً";
                    return;
                }
                
                IsLoading = true;
                
                var detail = new InventoryOpeningDetail
                {
                    DocID = CurrentHeader.DocID,
                    ItemID = NewLineItem!.ItemID,
                    UnitID = NewLineUnit!.UnitID,
                    NumericUnitID = NewLineNumericUnit?.UnitID,
                    Qty = NewLineQty,
                    NumericQty = NewLineNumericQty,
                    WarehouseId = NewLineWarehouse!.WarehouseID,
                    ExpiryDate = NewLineExpiryDate,
                    BatchNo = NewLineBatchNo,
                    SerialNo = NewLineSerialNo,
                    InitialUnitCost = NewLineUnitCost,
                    LineNotes = NewLineNotes
                };
                
                if (IsEditing && SelectedDetail != null)
                {
                    detail.LineID = SelectedDetail.LineID;
                    var updatedDetail = await _inventoryService.UpdateDetailAsync(detail);
                    // عيّن المراجع لعرض الأسماء مباشرة
                    updatedDetail.Item = NewLineItem;
                    updatedDetail.Warehouse = NewLineWarehouse;
                    updatedDetail.Unit = NewLineUnit;
                    
                    var index = Details.IndexOf(SelectedDetail);
                    Details[index] = updatedDetail;
                    
                    StatusMessage = "تم تحديث السطر";
                    IsEditing = false;
                }
                else
                {
                    var addedDetail = await _inventoryService.AddDetailAsync(detail);
                    // عيّن المراجع لعرض الأسماء مباشرة
                    addedDetail.Item = NewLineItem;
                    addedDetail.Warehouse = NewLineWarehouse;
                    addedDetail.Unit = NewLineUnit;
                    Details.Add(addedDetail);
                    StatusMessage = "تم إضافة السطر";
                }
                
                ClearNewLineData();
                SelectedDetail = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في إضافة/تحديث السطر: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteDetailAsync(InventoryOpeningDetail? detail)
        {
            try
            {
                if (detail == null) return;
                
                var result = MessageBox.Show(
                    $"هل تريد حذف السطر {detail.ItemInfo}؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result != MessageBoxResult.Yes) return;
                
                IsLoading = true;
                
                var success = await _inventoryService.DeleteDetailAsync(detail.LineID);
                
                if (success)
                {
                    Details.Remove(detail);
                    StatusMessage = "تم حذف السطر";
                    
                    if (SelectedDetail == detail)
                    {
                        SelectedDetail = null;
                        ClearNewLineData();
                        IsEditing = false;
                    }
                }
                else
                {
                    StatusMessage = "فشل في حذف السطر";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في الحذف: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            ClearNewLineData();
            SelectedDetail = null;
            IsEditing = false;
            StatusMessage = "تم إلغاء التعديل";
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحديث البيانات...";
                
                await LoadLookupDataAsync();
                
                StatusMessage = "تم تحديث البيانات بنجاح";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحديث البيانات: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"RefreshDataAsync Error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Helper Methods

        private async Task LoadLookupDataAsync()
        {
            try
            {
                StatusMessage = "جاري تحميل الأصناف...";
                
                // تحميل الأصناف من خدمة الأصناف
                var items = await _itemsService.GetAllAsync();
                AvailableItems.Clear();
                
                if (items?.Any() == true)
                {
                    foreach (var item in items.Where(i => i.IsActive == true))
                    {
                        AvailableItems.Add(item);
                    }
                    StatusMessage = $"تم تحميل {AvailableItems.Count} صنف";
                }
                else
                {
                    StatusMessage = "لا توجد أصناف متاحة";
                }
                
                StatusMessage = "جاري تحميل المخازن...";
                
                // تحميل المخازن من خدمة المخازن
                var warehouses = await _warehouseService.GetAllAsync();
                AvailableWarehouses.Clear();
                
                if (warehouses?.Any() == true)
                {
                    foreach (var warehouse in warehouses.Where(w => w.IsActive == true))
                    {
                        AvailableWarehouses.Add(warehouse);
                    }
                    StatusMessage = $"تم تحميل {AvailableWarehouses.Count} مخزن";
                }
                else
                {
                    StatusMessage = "لا توجد مخازن متاحة";
                }
                
                StatusMessage = "جاري تحميل الوحدات...";
                
                // تحميل الوحدات من خدمة الوحدات
                var units = await _unitsService.GetAllAsync();
                AvailableUnits.Clear();
                
                if (units?.Any() == true)
                {
                    foreach (var unit in units.Where(u => u.IsActive == true))
                    {
                        AvailableUnits.Add(unit);
                    }
                    StatusMessage = $"تم تحميل {AvailableUnits.Count} وحدة";
                }
                else
                {
                    StatusMessage = "لا توجد وحدات متاحة";
                }
                
                StatusMessage = $"تم تحميل البيانات بنجاح - الأصناف: {AvailableItems.Count}, المخازن: {AvailableWarehouses.Count}, الوحدات: {AvailableUnits.Count}";
                IsDataLoaded = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحميل البيانات: {ex.Message}";
                IsDataLoaded = false;
                
                // تسجيل تفاصيل إضافية للمطور
                System.Diagnostics.Debug.WriteLine($"LoadLookupDataAsync Error: {ex}");
            }
        }

        private async Task LoadItemUnitsAsync(int itemId)
        {
            try
            {
                // تحميل وحدات الصنف المحددة
                var itemDetails = await _itemsService.GetByIdAsync(itemId);
                if (itemDetails != null)
                {
                    // إضافة الوحدة الأساسية للصنف
                    if (itemDetails.Unit != null && !AvailableUnits.Contains(itemDetails.Unit))
                    {
                        AvailableUnits.Add(itemDetails.Unit);
                    }
                    
                    // تحميل وحدات إضافية للصنف من الخدمة
                    var itemUnits = await _itemsService.GetItemUnitsAsync(itemId);
                    if (itemUnits?.Any() == true)
                    {
                        foreach (var itemUnit in itemUnits)
                        {
                            if (itemUnit.Unit != null && !AvailableUnits.Contains(itemUnit.Unit))
                            {
                                AvailableUnits.Add(itemUnit.Unit);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحميل وحدات الصنف: {ex.Message}";
            }
        }

        private bool ValidateNewLineData()
        {
            // التحقق من تحميل البيانات أولاً
            if (!IsDataLoaded)
            {
                StatusMessage = "يجب تحميل البيانات أولاً. اضغط على زر 'تحديث البيانات'";
                return false;
            }

            if (!AvailableItems.Any())
            {
                StatusMessage = "لا توجد أصناف متاحة. تأكد من إضافة أصناف في النظام";
                return false;
            }

            if (!AvailableWarehouses.Any())
            {
                StatusMessage = "لا توجد مخازن متاحة. تأكد من إضافة مخازن في النظام";
                return false;
            }

            if (NewLineItem == null)
            {
                StatusMessage = "يجب اختيار صنف";
                return false;
            }
            
            if (NewLineWarehouse == null)
            {
                StatusMessage = "يجب اختيار مخزن";
                return false;
            }
            
            if (NewLineUnit == null)
            {
                StatusMessage = "يجب اختيار وحدة";
                return false;
            }
            
            if (NewLineQty < 0)
            {
                StatusMessage = "الكمية لا يمكن أن تكون سالبة";
                return false;
            }
            
            if (CurrentHeader.UseExpiry && NewLineExpiryDate == null)
            {
                StatusMessage = "تاريخ الانتهاء مطلوب";
                return false;
            }
            
            if (CurrentHeader.UseBatch && string.IsNullOrWhiteSpace(NewLineBatchNo))
            {
                StatusMessage = "رقم الدفعة مطلوب";
                return false;
            }
            
            if (CurrentHeader.UseSerial && string.IsNullOrWhiteSpace(NewLineSerialNo))
            {
                StatusMessage = "الرقم التسلسلي مطلوب";
                return false;
            }
            
            // السماح بتكلفة صفرية في الرصيد الافتتاحي
            return true;
        }

        private void ClearNewLineData()
        {
            NewLineItem = null;
            NewLineWarehouse = null;
            NewLineUnit = null;
            NewLineNumericUnit = null;
            NewLineQty = 0;
            NewLineNumericQty = null;
            NewLineUnitCost = null;
            NewLineExpiryDate = null;
            NewLineBatchNo = null;
            NewLineSerialNo = null;
            NewLineNotes = null;
        }

        #endregion
    }
}
