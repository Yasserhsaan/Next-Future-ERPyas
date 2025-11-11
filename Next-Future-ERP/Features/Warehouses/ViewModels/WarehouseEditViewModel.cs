using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Warehouses.ViewModels
{
    public partial class WarehouseEditViewModel : ObservableObject
    {
        private readonly IWarehouseService _service;
        private readonly IOrgLookupService _lookupService;

        [ObservableProperty] private Warehouse model;
        public ObservableCollection<LookupItem> Companies { get; } = new();
        public ObservableCollection<LookupItem> Branches { get; } = new();
        public ObservableCollection<LookupItem> CostCenters { get; } = new();
        public ObservableCollection<string> WarehouseTypes { get; } = new();

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => Model.WarehouseID == 0 ? "مستودع جديد" : "تعديل المستودع";

        public WarehouseEditViewModel(IWarehouseService service, IOrgLookupService lookupService, Warehouse model)
        {
            _service = service;
            _lookupService = lookupService;
            Model = Clone(model);
            _ = LoadLookupsAsync();
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                // تحميل الشركات
                Companies.Clear();
                var companies = await _lookupService.GetCompaniesAsync();
                foreach (var c in companies) Companies.Add(c);

                // تحميل الفروع
                Branches.Clear();
                if (Model.CompanyId.HasValue)
                {
                    var branches = await _lookupService.GetBranchesByCompanyAsync(Model.CompanyId.Value);
                    foreach (var b in branches) Branches.Add(b);
                }

                // تحميل مراكز التكلفة
                CostCenters.Clear();
                var costCenters = await _lookupService.GetInventoryCostCentersAsync();
                foreach (var cc in costCenters) CostCenters.Add(cc);

                // أنواع المستودعات
                WarehouseTypes.Clear();
                WarehouseTypes.Add("رئيسي");
                WarehouseTypes.Add("فرعي");
                WarehouseTypes.Add("مؤقت");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات المرجعية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Model.WarehouseCode))
                {
                    MessageBox.Show("كود المستودع مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(Model.WarehouseName))
                {
                    MessageBox.Show("اسم المستودع مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _service.UpsertAsync(Clone(Model));
                MessageBox.Show("تم حفظ المستودع بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);

                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand] 
        public void Cancel() => CloseRequested?.Invoke(this, false);

        private static Warehouse Clone(Warehouse model) => new()
        {
            WarehouseID = model.WarehouseID,
            WarehouseCode = model.WarehouseCode,
            WarehouseName = model.WarehouseName,
            Location = model.Location,
            WarehouseType = model.WarehouseType,
            ManagerID = model.ManagerID,
            IsActive = model.IsActive,
            IsDefault = model.IsDefault,
            AllowNegativeStock = model.AllowNegativeStock,
            UseBins = model.UseBins,
            CompanyId = model.CompanyId,
            BranshId = model.BranshId,
            DefultCostCenter = model.DefultCostCenter,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate,
            CreatedBy = model.CreatedBy,
            ModifiedBy = model.ModifiedBy
        };
    }
}
