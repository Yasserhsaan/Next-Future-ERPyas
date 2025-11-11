using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items; // For ItemPriceDto
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Items.ViewModels; // IUnitsLookupService (كما في مشروعك)
using Next_Future_ERP.Features.Warehouses.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Next_Future_ERP.Features.Items.ViewModels
{
    public partial class ItemPricesViewModel : ObservableObject
    {
        private readonly IItemPricesService _service;
        private readonly IUnitsLookupService _unitsLookup;

        public ObservableCollection<ItemPriceDto> Items { get; } = new();

        public ObservableCollection<LookupOption> AllItems { get; } = new();
        public ObservableCollection<UnitModel> AllUnits { get; } = new();

        public IReadOnlyList<LookupOption> PriceTypes { get; } = new[]
        {
            new LookupOption{ Id = 1, Name = "آخر سعر شراء" },
            new LookupOption{ Id = 2, Name = "أعلى سعر شراء" },
            new LookupOption{ Id = 3, Name = "المتوسط"      }
        };
        public IReadOnlyList<LookupOption> PriceLevels { get; } = new[]
        {
            new LookupOption{ Id = 1, Name = "تجزئة"  },
            new LookupOption{ Id = 2, Name = "جملة"   },
            new LookupOption{ Id = 3, Name = "افتراضي"}
        };
        public IReadOnlyList<LookupOption> Methods { get; } = new[]
        {
            new LookupOption{ Id = 1, Name = "يدوي"  },
            new LookupOption{ Id = 2, Name = "نسبة"  },
            new LookupOption{ Id = 3, Name = "تلقائي"}
        };

        // فلاتر (لو تحتاجها بالعلوي—أبقيناها للاتساق)
        [ObservableProperty] private string? searchText;
        [ObservableProperty] private int? selectedPriceType;
        [ObservableProperty] private int? selectedPriceLevel;
        [ObservableProperty] private int? selectedMethod;
        [ObservableProperty] private bool isActiveOnly;
        [ObservableProperty] private DateTime? dateFrom;
        [ObservableProperty] private DateTime? dateTo;

        [ObservableProperty] private ItemPriceDto? selectedRow;

        [ObservableProperty]
        private ItemPrice edit = new()
        {
            IsActive = true,
            EffectiveFrom = DateTime.Today,
            PriceType = 1,
            PriceLevelId = 1,
            Method = 1,
            CompanyId = 1, // TODO: Get from current session
            BranchId = 1,  // TODO: Get from current session
            CurrencyId = 1, // TODO: Get default currency
            CreatedBy = 1   // TODO: Get from current user
        };

        // لربط ComboBox الصنف في البطاقة
        [ObservableProperty] private int? selectedItemIdForEdit;
        
        // للتحكم في إظهار/إخفاء ComboBox الصنف
        [ObservableProperty] private bool showItemSelector = true;
        
        // لمنع التحميل المتكرر
        private bool _isInitialized = false;

        public ItemPricesViewModel(IItemPricesService service, IUnitsLookupService unitsLookup)
        {
            try
            {
                _service = service;
                _unitsLookup = unitsLookup;

                // تأجيل تحميل البيانات حتى يتم استدعاء SetContext
                // _ = LoadLookupsAsync();
                // _ = LoadAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"خطأ في إنشاء ItemPricesViewModel: {ex.Message}", "خطأ", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // Method لتعيين السياق - هل نحن في تاب أم شاشة مستقلة
        public void SetContext(bool isTab, int? itemId = null)
        {
            try
            {
                ShowItemSelector = !isTab; // إخفاء ComboBox الصنف في التاب
                
                // تحميل البيانات المرجعية مرة واحدة فقط
                if (!_isInitialized)
                {
                    _ = LoadLookupsAsync();
                    _isInitialized = true;
                }
                
                if (isTab && itemId.HasValue)
                {
                    SelectedItemIdForEdit = itemId.Value;
                    // تحميل البيانات المرتبطة بالصنف
                    _ = LoadAsync();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"خطأ في SetContext: {ex.Message}", "خطأ", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                // تحميل البيانات المرجعية بشكل متسلسل
                var itemsLookup = await _service.GetItemsLookupAsync();
                var units = await _unitsLookup.GetAllAsync();
                
                // تحديث المجموعات على UI thread بشكل آمن
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        AllItems.Clear();
                        foreach (var (id, display) in itemsLookup)
                            AllItems.Add(new LookupOption { Id = id, Name = display });

                        AllUnits.Clear();
                        foreach (var u in units.OrderBy(x => x.UnitName))
                            AllUnits.Add(u);
                    }
                    catch (Exception uiEx)
                    {
                        System.Windows.MessageBox.Show($"خطأ في تحديث الواجهة: {uiEx.Message}", "خطأ", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    System.Windows.MessageBox.Show($"خطأ في تحميل البيانات المرجعية: {ex.Message}", "خطأ", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                });
            }
        }

        [RelayCommand] public async Task RefreshAsync() => await LoadAsync();

        [RelayCommand]
        public async Task ClearFiltersAsync()
        {
            SearchText = null; SelectedPriceType = null; SelectedPriceLevel = null; SelectedMethod = null;
            IsActiveOnly = false; DateFrom = null; DateTo = null;
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var list = await _service.GetAllAsync(
                    SearchText, SelectedPriceType, SelectedPriceLevel, SelectedMethod,
                    IsActiveOnly, DateFrom, DateTo,
                    SelectedItemIdForEdit);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Items.Clear();
                    foreach (var r in list) Items.Add(r);
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    System.Windows.MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                });
            }
        }

        partial void OnSelectedRowChanged(ItemPriceDto? oldValue, ItemPriceDto? newValue)
        {
            if (newValue is null) return;

            Edit = new ItemPrice
            {
                PriceID = newValue.PriceID,
                CompanyId = newValue.CompanyId,
                BranchId = newValue.BranchId,
                ItemID = newValue.ItemID,
                UnitID = newValue.UnitID,
                PriceLevelId = newValue.PriceLevelId,
                CurrencyId = newValue.CurrencyId,
                PriceType = newValue.PriceType,
                Method = newValue.Method,
                PriceAmount = newValue.PriceAmount,
                PricePercent = newValue.PricePercent,
                SellPrice = newValue.SellPrice,
                EffectiveFrom = newValue.EffectiveFrom,
                EffectiveTo = newValue.EffectiveTo,
                IsActive = newValue.IsActive,
                IsDefault = newValue.IsDefault,
                CreatedBy = newValue.CreatedBy,
                CreatedAt = newValue.CreatedAt
            };

            SelectedItemIdForEdit = newValue.ItemID;
        }

        partial void OnSelectedItemIdForEditChanged(int? oldValue, int? newValue)
        {
            if (newValue is not int id) return;

            Edit.ItemID = id;

            var last = Items
                .Where(x => x.ItemID == id)
                .OrderByDescending(x => x.ModifiedAt ?? x.CreatedAt)
                .FirstOrDefault();

            if (last != null)
                OnSelectedRowChanged(SelectedRow, last);
            else
            {
                Edit = new ItemPrice
                {
                    ItemID = id,
                    IsActive = true,
                    EffectiveFrom = DateTime.Today,
                    PriceType = 1,
                    PriceLevelId = 1,
                    Method = 1,
                    CompanyId = 1,
                    BranchId = 1,
                    CurrencyId = 1,
                    CreatedBy = 1
                };
            }
        }

        [RelayCommand]
        public void New()
        {
            SelectedRow = null;
            Edit = new ItemPrice
            {
                IsActive = true,
                EffectiveFrom = DateTime.Today,
                PriceType = 1,
                PriceLevelId = 1,
                Method = 1,
                CompanyId = 1,
                BranchId = 1,
                CurrencyId = 1,
                CreatedBy = 1,
                ItemID = SelectedItemIdForEdit ?? 0,  // الحفاظ على الصنف المحدد
                UnitID = AllUnits.FirstOrDefault()?.UnitID ?? 0  // اختيار أول وحدة افتراضياً
            };
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                // التحقق من البيانات المطلوبة
                if (Edit.ItemID <= 0)
                {
                    System.Windows.MessageBox.Show("يرجى اختيار الصنف أولاً", "خطأ في البيانات", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (Edit.UnitID <= 0)
                {
                    System.Windows.MessageBox.Show("يرجى اختيار الوحدة", "خطأ في البيانات", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // حساب سعر البيع حسب الطريقة المختارة
                if (Edit.Method == 1) // يدوي
                    Edit.SellPrice = Edit.PriceAmount ?? 0m;
                else if (Edit.Method == 2) // نسبة
                {
                    var baseVal = Edit.PriceAmount ?? 0m;
                    var pct = (Edit.PricePercent ?? 0) / 100m;
                    Edit.SellPrice = baseVal + (baseVal * pct);
                }
                
                // تعيين قيم إضافية مطلوبة
                if (Edit.PriceID == 0)
                {
                    Edit.CreatedAt = DateTime.Now;
                    Edit.CreatedBy = 1; // TODO: Get from current user
                }
                else
                {
                    Edit.ModifiedAt = DateTime.Now;
                    Edit.ModifiedBy = 1; // TODO: Get from current user
                }

                if (Edit.PriceID == 0)
                    Edit.PriceID = await _service.AddAsync(Edit);
                else
                    await _service.UpdateAsync(Edit);

                await LoadAsync();
                
                System.Windows.MessageBox.Show("تم حفظ السعر بنجاح", "نجح الحفظ", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (DbUpdateException ex)
            {
                System.Windows.MessageBox.Show(ex.GetBaseException().Message, "EF DbUpdateException");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "خطأ أثناء الحفظ");
            }
        }

        [RelayCommand]
        public void EditPrice(ItemPriceDto? priceDto = null)
        {
            var price = priceDto ?? SelectedRow;
            if (price == null) return;
            
            SelectedRow = price;
            // سيتم تحديث Edit تلقائياً عبر OnSelectedRowChanged
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        public async Task DeleteAsync(ItemPriceDto? priceDto = null)
        {
            var priceToDelete = priceDto?.PriceID ?? Edit?.PriceID ?? 0;
            if (priceToDelete <= 0) return;
            
            var result = System.Windows.MessageBox.Show(
                "هل أنت متأكد من حذف هذا السعر؟",
                "تأكيد الحذف",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);
                
            if (result != System.Windows.MessageBoxResult.Yes) return;
            
            try
            {
                await _service.DeleteAsync(priceToDelete);
                New();
                await LoadAsync();
            }
            catch (DbUpdateException ex)
            {
                System.Windows.MessageBox.Show(ex.GetBaseException().Message, "EF DbUpdateException");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "خطأ أثناء الحذف");
            }
        }
        private bool CanDelete() => Edit?.PriceID > 0;
    }
}
