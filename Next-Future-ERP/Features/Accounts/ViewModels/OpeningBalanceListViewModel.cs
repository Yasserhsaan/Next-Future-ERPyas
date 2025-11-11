using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.Accounts.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class OpeningBalanceListViewModel : ObservableObject
    {
        private readonly IOpeningBalanceService _service;

        public ObservableCollection<OpeningBalanceBatch> Items { get; } = new();

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private OpeningBalanceBatch? selectedItem;
        
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

        public OpeningBalanceListViewModel(IOpeningBalanceService service)
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

        partial void OnSelectedItemChanged(OpeningBalanceBatch? oldValue, OpeningBalanceBatch? newValue)
            => EditDialogCommand.NotifyCanExecuteChanged();

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            Items.Clear();
            
            // استخدام SearchBatchesAsync مع filter فارغ للحصول على كل الدفعات
            var filter = new OpeningBalanceSearchFilter();
            var list = await _service.SearchBatchesAsync(filter);
            
            // تطبيق البحث إذا كان موجوداً
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                list = list.Where(b => 
                    b.DocNo?.ToLower().Contains(searchLower) == true ||
                    b.Description?.ToLower().Contains(searchLower) == true ||
                    b.BatchId.ToString().Contains(searchLower) == true
                ).ToList();
            }
            
            // تحميل السطور لكل دفعة
            foreach (var batch in list)
            {
                var lines = await _service.GetBatchLinesAsync(batch.BatchId);
                batch.Lines = lines;
                Items.Add(batch);
            }
        }

        private void OpenNewDialog()
        {
            var vm = App.ServiceProvider.GetRequiredService<OpeningBalanceEditViewModel>();
            vm.InitializeNew();
            var win = new OpeningBalanceEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var vm = App.ServiceProvider.GetRequiredService<OpeningBalanceEditViewModel>();
            var cloned = Clone(SelectedItem);
            vm.InitializeEdit(cloned);
            var win = new OpeningBalanceEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف دفعة الأرصدة الافتتاحية المحددة؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteDraftAsync(SelectedItem.BatchId);
                await LoadAsync();
            }
        }

        private static OpeningBalanceBatch Clone(OpeningBalanceBatch model) => new()
        {
            BatchId = model.BatchId,
            CompanyId = model.CompanyId,
            BranchId = model.BranchId,
            FiscalYear = model.FiscalYear,
            DocNo = model.DocNo,
            DocDate = model.DocDate,
            Description = model.Description,
            Status = model.Status,
            CreatedBy = model.CreatedBy,
            CreatedAt = model.CreatedAt,
            PostedBy = model.PostedBy,
            PostedAt = model.PostedAt,
            Lines = model.Lines?.Select(l => new OpeningBalanceLine
            {
                LineId = l.LineId,
                BatchId = l.BatchId,
                AccountId = l.AccountId,
                CostCenterId = l.CostCenterId,
                TransactionCurrencyId = l.TransactionCurrencyId,
                TransactionDebit = l.TransactionDebit,
                TransactionCredit = l.TransactionCredit,
                CompanyCurrencyId = l.CompanyCurrencyId,
                CompanyDebit = l.CompanyDebit,
                CompanyCredit = l.CompanyCredit,
                ExchangeRate = l.ExchangeRate,
                Note = l.Note,
                AccountCode = l.AccountCode,
                AccountNameAr = l.AccountNameAr,
                UsesCostCenter = l.UsesCostCenter,
                CostCenterName = l.CostCenterName,
                TransactionCurrencyName = l.TransactionCurrencyName,
                CompanyCurrencyName = l.CompanyCurrencyName
            }).ToList() ?? new List<OpeningBalanceLine>()
        };
    }
}

