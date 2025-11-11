using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Models;
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
    public partial class DebitCreditNotificationListViewModel : ObservableObject
    {
        private readonly DebitCreditNotificationService _service;

        public ObservableCollection<DebitCreditNotificationLookupItem> Items { get; } = new();

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private DebitCreditNotificationLookupItem? selectedItem;
        
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

        public DebitCreditNotificationListViewModel(DebitCreditNotificationService service)
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

        partial void OnSelectedItemChanged(DebitCreditNotificationLookupItem? oldValue, DebitCreditNotificationLookupItem? newValue)
            => EditDialogCommand.NotifyCanExecuteChanged();

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            Items.Clear();
            
            // استخدام SearchAsync مع فلاتر فارغة للحصول على كل الإشعارات
            var (items, total) = await _service.SearchAsync(
                branchId: null,
                dcType: null,
                accountNumber: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                dateFrom: null,
                dateTo: null,
                status: null,
                skip: 0,
                take: 1000 // عدد كبير للحصول على كل الإشعارات
            );
            
            // تطبيق البحث إذا كان موجوداً
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                items = items.Where(v => 
                    v.AccountNumber?.ToLower().Contains(searchLower) == true ||
                    v.NotificationId.ToString().Contains(searchLower) == true ||
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
            var vm = App.ServiceProvider.GetRequiredService<DebitCreditNotificationEditViewModel>();
            vm.InitializeNew();
            var win = new DebitCreditNotificationEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var vm = App.ServiceProvider.GetRequiredService<DebitCreditNotificationEditViewModel>();
            vm.InitializeEdit(SelectedItem.NotificationId);
            var win = new DebitCreditNotificationEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف الإشعار المحدد؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.NotificationId);
                await LoadAsync();
            }
        }
    }
}

