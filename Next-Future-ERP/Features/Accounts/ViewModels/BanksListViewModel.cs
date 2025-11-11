using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.Accounts.Views;
using Next_Future_ERP.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class BanksListViewModel : ObservableObject
    {
        private readonly BankService _service;

        public ObservableCollection<Bank> Items { get; } = new();

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private Bank? selectedItem;
        
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

        public BanksListViewModel(BankService service)
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

        partial void OnSelectedItemChanged(Bank? oldValue, Bank? newValue)
            => EditDialogCommand.NotifyCanExecuteChanged();

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            Items.Clear();
            var list = await _service.GetAllAsync();
            
            // تطبيق البحث إذا كان موجوداً
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                list = list.Where(b => 
                    b.BankName?.ToLower().Contains(searchLower) == true ||
                    b.AccountNumber?.ToLower().Contains(searchLower) == true ||
                    b.Company?.CompName?.ToLower().Contains(searchLower) == true ||
                    b.Branch?.BranchName?.ToLower().Contains(searchLower) == true
                ).ToList();
            }
            
            foreach (var item in list) Items.Add(item);
        }

        private void OpenNewDialog()
        {
            var vm = App.ServiceProvider.GetRequiredService<BankEditViewModel>();
            vm.InitializeNew();
            var win = new BankEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var vm = App.ServiceProvider.GetRequiredService<BankEditViewModel>();
            var cloned = Clone(SelectedItem);
            vm.InitializeEdit(cloned);
            var win = new BankEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف البنك المحدد؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.BankId);
                await LoadAsync();
            }
        }

        private static Bank Clone(Bank model) => new()
        {
            BankId = model.BankId,
            BankName = model.BankName,
            AccountNumber = model.AccountNumber,
            CompanyId = model.CompanyId,
            BranchId = model.BranchId,
            IsActive = model.IsActive,
            StopDate = model.StopDate,
            StopReason = model.StopReason,
            ContactInfo = model.ContactInfo,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CurrencyDetails = model.CurrencyDetails?.Select(cd => new BankCurrencyDetail
            {
                DetailId = cd.DetailId,
                BankId = cd.BankId,
                CurrencyId = cd.CurrencyId,
                BankAccountNumber = cd.BankAccountNumber,
                MinCash = cd.MinCash,
                MaxCash = cd.MaxCash,
                MinTransaction = cd.MinTransaction,
                MaxTransaction = cd.MaxTransaction,
                AllowLimitExceed = cd.AllowLimitExceed,
                CreatedAt = cd.CreatedAt
            }).ToList() ?? new List<BankCurrencyDetail>()
        };
    }
}

