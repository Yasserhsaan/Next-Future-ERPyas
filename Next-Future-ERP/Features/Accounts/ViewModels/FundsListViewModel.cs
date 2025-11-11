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
    public partial class FundsListViewModel : ObservableObject
    {
        private readonly FundService _service;

        public ObservableCollection<Fund> Items { get; } = new();

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private Fund? selectedItem;
        
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

        public FundsListViewModel(FundService service)
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

        partial void OnSelectedItemChanged(Fund? oldValue, Fund? newValue)
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
                list = list.Where(f => 
                    f.FundName?.ToLower().Contains(searchLower) == true ||
                    f.AccountNumber?.ToLower().Contains(searchLower) == true ||
                    f.Company?.CompName?.ToLower().Contains(searchLower) == true ||
                    f.Branch?.BranchName?.ToLower().Contains(searchLower) == true
                ).ToList();
            }
            
            foreach (var item in list) Items.Add(item);
        }

        private void OpenNewDialog()
        {
            var vm = App.ServiceProvider.GetRequiredService<FundEditViewModel>();
            vm.InitializeNew();
            var win = new FundEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var vm = App.ServiceProvider.GetRequiredService<FundEditViewModel>();
            var cloned = Clone(SelectedItem);
            vm.InitializeEdit(cloned);
            var win = new FundEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف الصندوق المحدد؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.FundId);
                await LoadAsync();
            }
        }

        private static Fund Clone(Fund model) => new()
        {
            FundId = model.FundId,
            FundName = model.FundName,
            AccountNumber = model.AccountNumber,
            CompanyId = model.CompanyId,
            BranchId = model.BranchId,
            FundType = model.FundType,
            IsActive = model.IsActive,
            IsUsed = model.IsUsed,
            StopDate = model.StopDate,
            StopReason = model.StopReason,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CurrencyLimits = model.CurrencyLimits?.Select(cl => new FundCurrencyLimit
            {
                LimitId = cl.LimitId,
                FundId = cl.FundId,
                CurrencyId = cl.CurrencyId,
                MinCash = cl.MinCash,
                MaxCash = cl.MaxCash,
                MinSettlement = cl.MinSettlement,
                MaxSettlement = cl.MaxSettlement,
                AllowLimitExceed = cl.AllowLimitExceed,
                CreatedAt = cl.CreatedAt
            }).ToList() ?? new List<FundCurrencyLimit>()
        };
    }
}

