using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
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
    public partial class ValuationGroupViewModel : ObservableObject
    {
        private readonly IValuationGroupService _service;
        private readonly AccountsService _accounts = new();

        public ObservableCollection<ValuationGroup> Items { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private ValuationGroup? selectedItem;
        
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

        public ValuationGroupViewModel(IValuationGroupService service)
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

        partial void OnSelectedItemChanged(ValuationGroup? oldValue, ValuationGroup? newValue)
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
                        (x.ValuationGroupCode ?? "").Contains(s)
                        || (x.ValuationGroupName ?? "").Contains(s)
                        || (x.Description ?? "").Contains(s)
                    ).ToList();
                }
                
                foreach (var vg in list.OrderBy(x => x.ValuationGroupName)) Items.Add(vg);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ أثناء تحميل البيانات: " + ex.Message);
            }
        }

        private void OpenNewDialog()
        {
            var vm = new ValuationGroupEditViewModel(_service, _accounts, new ValuationGroup
            {
                IsActive = true
            });
            var win = new ValuationGroupEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var vm = new ValuationGroupEditViewModel(_service, _accounts, Clone(SelectedItem));
            var win = new ValuationGroupEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف مجموعة التقييم المحددة؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.ValuationGroupId);
                await LoadAsync();
            }
        }

        private static ValuationGroup Clone(ValuationGroup model) => new()
        {
            ValuationGroupId = model.ValuationGroupId,
            CompanyId = model.CompanyId,
            ValuationGroupCode = model.ValuationGroupCode,
            ValuationGroupName = model.ValuationGroupName,
            Description = model.Description,
            IsActive = model.IsActive,
            CreatedDate = model.CreatedDate,
            ModifiedDate = model.ModifiedDate,
            CreatedBy = model.CreatedBy,
            ModifiedBy = model.ModifiedBy,
            CostCenterId = model.CostCenterId
        };
    }
}
