using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Payments.Services;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Next_Future_ERP.Features.Suppliers.Views;

namespace Next_Future_ERP.Features.Suppliers.ViewModels
{
    public partial class SuppliersViewModel : ObservableObject
    {
        private readonly ISuppliersService _service;
        private readonly IPaymentMethodsService _pmService;
        private readonly IPaymentTermsService _termsService;
        private readonly AccountsService _accounts = new();

        public ObservableCollection<Supplier> Items { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private Supplier? selectedItem;
        
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

        public SuppliersViewModel(
            ISuppliersService service,
            IPaymentMethodsService pmService,
            IPaymentTermsService termsService)
        {
            _service = service;
            _pmService = pmService;
            _termsService = termsService;

            // تهيئة الأوامر
            _newDialogCommand = new RelayCommand(OpenNewDialog);
            _editDialogCommand = new RelayCommand(OpenEditDialog, () => SelectedItem != null);
            _deleteCommand = new AsyncRelayCommand(DeleteAsync);
            _refreshCommand = new AsyncRelayCommand(LoadAsync);
            _loadCommand = new AsyncRelayCommand(LoadAsync);
            
            _ = LoadAsync();
        }

        partial void OnSelectedItemChanged(Supplier? oldValue, Supplier? newValue)
            => EditDialogCommand.NotifyCanExecuteChanged();

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            Items.Clear();
            var list = await _service.GetAllAsync(SearchText);
            foreach (var item in list) Items.Add(item);
        }

        private void OpenNewDialog()
        {
            var vm = new SupplierEditViewModel(_service, _pmService, _termsService, _accounts, new Supplier { IsActive = true });
            var win = new SupplierEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var vm = new SupplierEditViewModel(_service, _pmService, _termsService, _accounts, Clone(SelectedItem));
            var win = new SupplierEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف المورد المحدد؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.SupplierID);
                await LoadAsync();
            }
        }

        private static Supplier Clone(Supplier model) => new()
        {
            SupplierID = model.SupplierID,
            SupplierCode = model.SupplierCode,
            SupplierName = model.SupplierName,
            TaxNumber = model.TaxNumber,
            AccountID = model.AccountID,
            CostCenterID = model.CostCenterID,
            PaymentTerms = model.PaymentTerms,
            CreditLimit = model.CreditLimit,
            ContactPerson = model.ContactPerson,
            Phone = model.Phone,
            Email = model.Email,
            Address = model.Address,
            IsActive = model.IsActive,
            Nationality = model.Nationality,
            IDNumber = model.IDNumber,
            CRNumber = model.CRNumber,
            VATNumber = model.VATNumber,
            DefaultPaymentMethodID = model.DefaultPaymentMethodID
        };
    }
}
