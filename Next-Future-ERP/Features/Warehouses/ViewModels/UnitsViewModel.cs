using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.ViewModels
{
    public partial class UnitsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<UnitModel> units = new();

        [ObservableProperty]
        private UnitModel selectedUnit = new();

        [ObservableProperty]
        private List<UnitTypesModel> unitTypes = new()
        {
            new UnitTypesModel { Key = '1', Values = "عددية" },
            new UnitTypesModel { Key = '2', Values = "مقاسة" }
        };

        [ObservableProperty]
        private ObservableCollection<UnitClasseModel> unitClasses = new()
        {
            new UnitClasseModel { Key = '0', Values = "اختر الفئة" },
            new UnitClasseModel { Key = '1', Values = "وزن" },
            new UnitClasseModel { Key = '2', Values = "مساحة" },
            new UnitClasseModel { Key = '3', Values = "أطوال" },
            new UnitClasseModel { Key = '4', Values = "سوائل" }
        };

        [ObservableProperty]
        private ObservableCollection<UnitModel> baseUnits = new();

        private static readonly ObservableCollection<UnitClasseModel> EmptyUnitClasses = new();

        public ObservableCollection<UnitClasseModel> FilteredUnitClasses =>
            SelectedUnit?.UnitType == '2' ? unitClasses : EmptyUnitClasses;

        public ObservableCollection<UnitModel> FilteredBaseUnits =>
            SelectedUnit?.UnitType != '\0' ? baseUnits : new();

        private readonly IUnitsService _unitsService;

        public UnitsViewModel(IUnitsService unitsService)
        {
            _unitsService = unitsService ?? throw new ArgumentNullException(nameof(unitsService));
            LoadSampleData();

            // Ensure the initial SelectedUnit is hooked for change notifications
            if (SelectedUnit is INotifyPropertyChanged initialNotify)
            {
                initialNotify.PropertyChanged += SelectedUnit_PropertyChanged;
                _previousSelectedUnit = initialNotify;
            }

            // Initialize dependent UI
            OnPropertyChanged(nameof(FilteredUnitClasses));
            OnPropertyChanged(nameof(FilteredBaseUnits));
        }

        private void LoadSampleData()
        {
            Units.Add(new UnitModel
            {
                UnitID = 1,
                UnitCode = "m",
                UnitName = "متر",
                ConversionFactor = 1m
            });

            Units.Add(new UnitModel
            {
                UnitID = 2,
                UnitCode = "cm",
                UnitName = "سنتيمتر",
                ConversionFactor = 0.01m
            });
        }

        [RelayCommand]
        private void AddNew()
        {
            SelectedUnit = new UnitModel();
        }

        [RelayCommand]
        private async Task Save()
        {
            await _unitsService.SaveAsync(SelectedUnit);
            await LoadByTypeCommand.ExecuteAsync(SelectedUnit.UnitType);
        }

        [RelayCommand]
        private async Task LoadByType(char unitType)
        {
            var list = await _unitsService.GetByUnitTypeAsync(unitType);
            Units.Clear();
            foreach (var u in list)
                Units.Add(u);

            // Load base units for the same type
            BaseUnits.Clear();
            foreach (var u in list)
                BaseUnits.Add(u);
        }

        [RelayCommand]
        private async Task LoadAll()
        {
            var list = await _unitsService.GetAllAsync();
            Units.Clear();
            foreach (var u in list)
                Units.Add(u);
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedUnit?.UnitID == 0)
            {
                System.Windows.MessageBox.Show("⚠️ يرجى اختيار وحدة للحذف.", "تنبيه", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var result = System.Windows.MessageBox.Show(
                $"هل أنت متأكد من حذف الوحدة '{SelectedUnit?.UnitName}'؟", 
                "تأكيد الحذف", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                await _unitsService.DeleteAsync(SelectedUnit.UnitID);
                await LoadByTypeCommand.ExecuteAsync(SelectedUnit.UnitType);
                SelectedUnit = new UnitModel();
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            SelectedUnit = new UnitModel();
        }

        private INotifyPropertyChanged? _previousSelectedUnit;

        partial void OnSelectedUnitChanged(UnitModel? value)
        {
            // Detach event handler from the previous selection
            if (_previousSelectedUnit != null)
            {
                _previousSelectedUnit.PropertyChanged -= SelectedUnit_PropertyChanged;
            }

            // Attach event handler to new selection
            if (value is INotifyPropertyChanged newNotify)
            {
                newNotify.PropertyChanged += SelectedUnit_PropertyChanged;
                _previousSelectedUnit = newNotify;
            }

            // Update UI
            OnPropertyChanged(nameof(FilteredUnitClasses));
            OnPropertyChanged(nameof(FilteredBaseUnits));
        }

        private void SelectedUnit_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UnitModel.UnitType))
            {
                // When UnitType changes, reset UnitClass and notify the UI
                if (SelectedUnit != null)
                {
                    SelectedUnit.UnitClass = '0';
                    SelectedUnit.BaseUnitID = null;
                    OnPropertyChanged(nameof(FilteredUnitClasses));
                    OnPropertyChanged(nameof(FilteredBaseUnits));
                    _ = LoadByType(SelectedUnit.UnitType);
                }
            }
        }

    }
}
