using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Next_Future_ERP.Features.Accounts.Views;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class AccountClassesViewModel : ObservableObject
    {
        private readonly IAccountClassService _service;

        public ObservableCollection<AccountClass> Items { get; } = new();

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private AccountClass? selectedItem;
        
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

        public AccountClassesViewModel(IAccountClassService service)
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

        partial void OnSelectedItemChanged(AccountClass? oldValue, AccountClass? newValue)
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
            var vm = new AccountClassEditViewModel(_service, new AccountClass());
            var win = new AccountClassEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var vm = new AccountClassEditViewModel(_service, Clone(SelectedItem));
            var win = new AccountClassEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف التصنيف المحدد؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.AccountClassId);
                await LoadAsync();
            }
        }

        private static AccountClass Clone(AccountClass model) => new()
        {
            AccountClassId = model.AccountClassId,
            AccountClassAname = model.AccountClassAname,
            AccountClassEname = model.AccountClassEname,
            CategoryKey = model.CategoryKey
        };
    }
}
