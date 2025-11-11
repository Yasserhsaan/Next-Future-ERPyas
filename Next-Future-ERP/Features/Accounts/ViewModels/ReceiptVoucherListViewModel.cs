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
    public partial class ReceiptVoucherListViewModel : ObservableObject
    {
        private readonly ReceiptVoucherService _service;

        public ObservableCollection<ReceiptVoucherLookupItem> Items { get; } = new();

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private ReceiptVoucherLookupItem? selectedItem;
        
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

        public ReceiptVoucherListViewModel(ReceiptVoucherService service)
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

        partial void OnSelectedItemChanged(ReceiptVoucherLookupItem? oldValue, ReceiptVoucherLookupItem? newValue)
            => EditDialogCommand.NotifyCanExecuteChanged();

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            Items.Clear();
            
            // استخدام SearchAsync مع فلاتر فارغة للحصول على كل السندات
            var (items, total) = await _service.SearchAsync(
                branchId: null,
                voucherType: null,
                sourceId: null,
                dateFrom: null,
                dateTo: null,
                docNo: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                beneficiary: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                skip: 0,
                take: 1000 // عدد كبير للحصول على كل السندات
            );
            
            // تطبيق البحث إذا كان موجوداً
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                items = items.Where(v => 
                    v.DocumentNumber?.ToLower().Contains(searchLower) == true ||
                    v.Beneficiary?.ToLower().Contains(searchLower) == true ||
                    v.VoucherID.ToString().Contains(searchLower) == true ||
                    v.BranchName?.ToLower().Contains(searchLower) == true
                ).ToList();
            }
            
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        private void OpenNewDialog()
        {
            var vm = App.ServiceProvider.GetRequiredService<ReceiptVoucherEditViewModel>();
            vm.InitializeNew();
            var win = new ReceiptVoucherEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var vm = App.ServiceProvider.GetRequiredService<ReceiptVoucherEditViewModel>();
            vm.InitializeEdit(SelectedItem.VoucherID);
            var win = new ReceiptVoucherEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف سند القبض المحدد؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.VoucherID);
                await LoadAsync();
            }
        }
    }
}

