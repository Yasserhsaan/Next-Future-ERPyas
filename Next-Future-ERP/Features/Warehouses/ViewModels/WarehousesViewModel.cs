using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Next_Future_ERP.Features.Warehouses.Views;

namespace Next_Future_ERP.Features.Warehouses.ViewModels
{
    public partial class WarehousesViewModel : ObservableObject
    {
        private readonly IWarehouseService _service;

        public ObservableCollection<Warehouse> Items { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private Warehouse? selectedItem;

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

        public WarehousesViewModel(IWarehouseService service)
        {
            _service = service;
            
            // تهيئة الأوامر
            _newDialogCommand = new RelayCommand(OpenNewDialog);
            _editDialogCommand = new RelayCommand(OpenEditDialog, () => SelectedItem != null);
            _deleteCommand = new AsyncRelayCommand(DeleteAsync);
            _refreshCommand = new AsyncRelayCommand(LoadAsync);
            _loadCommand = new AsyncRelayCommand(LoadAsync);

            _ = LoadAsync();
        }

        partial void OnSelectedItemChanged(Warehouse? oldValue, Warehouse? newValue)
            => EditDialogCommand.NotifyCanExecuteChanged();

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            try
            {
                Items.Clear();
                var list = await _service.GetAllAsync();
                
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var s = SearchText.Trim();
                    list = list.Where(x =>
                        (x.WarehouseCode ?? "").Contains(s)
                        || (x.WarehouseName ?? "").Contains(s)
                        || (x.Location ?? "").Contains(s)
                    ).ToList();
                }
                
                foreach (var warehouse in list.OrderBy(x => x.WarehouseName)) Items.Add(warehouse);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ أثناء تحميل البيانات: " + ex.Message);
            }
        }

        private void OpenNewDialog()
        {
            var lookupService = App.ServiceProvider.GetRequiredService<IOrgLookupService>();
            var vm = new WarehouseEditViewModel(_service, lookupService, new Warehouse
            {
                IsActive = true
            });
            var win = new WarehouseEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var lookupService = App.ServiceProvider.GetRequiredService<IOrgLookupService>();
            var vm = new WarehouseEditViewModel(_service, lookupService, Clone(SelectedItem));
            var win = new WarehouseEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف المستودع المحدد؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.WarehouseID);
                await LoadAsync();
            }
        }

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