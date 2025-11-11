using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Next_Future_ERP.Features.Accounts.Views;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class CurrencyExchangeRatesViewModel : ObservableObject
    {
        private readonly ICurrencyExchangeRateService _service;

        public ObservableCollection<CurrencyExchangeRate> Items { get; } = new();
        public ObservableCollection<NextCurrency> Currencies { get; } = new();

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private CurrencyExchangeRate? selectedItem;
        
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

        public CurrencyExchangeRatesViewModel(ICurrencyExchangeRateService service)
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

        partial void OnSelectedItemChanged(CurrencyExchangeRate? oldValue, CurrencyExchangeRate? newValue)
            => EditDialogCommand.NotifyCanExecuteChanged();

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            try
            {
                // عملات
                Currencies.Clear();
                var cur = await _service.GetActiveCurrenciesAsync();
                foreach (var c in cur.OrderBy(x => x.CurrencyNameAr)) Currencies.Add(c);

                // أسعار الصرف
                Items.Clear();
                var list = await _service.GetAllAsync();
                
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var s = SearchText.Trim();
                    list = list.Where(x =>
                        (x.Currency?.CurrencyNameAr ?? "").Contains(s)
                        || (x.Currency?.CurrencyNameEn ?? "").Contains(s)
                        || (x.Currency?.CurrencySymbol ?? "").Contains(s)
                    ).ToList();
                }
                
                foreach (var r in list.OrderByDescending(x => x.DateExchangeStart)) Items.Add(r);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ أثناء تحميل البيانات: " + ex.Message);
            }
        }

        private void OpenNewDialog()
        {
            var vm = new ExchangeRateEditViewModel(_service, new CurrencyExchangeRate
            {
                Status = true,
                DateExchangeStart = DateTime.Today,
                DateExchangeEnd = null
            });
            var win = new ExchangeRateEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var vm = new ExchangeRateEditViewModel(_service, Clone(SelectedItem));
            var win = new ExchangeRateEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف سعر الصرف المحدد؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.Id);
                await LoadAsync();
            }
        }

        private static CurrencyExchangeRate Clone(CurrencyExchangeRate model) => new()
        {
            Id = model.Id,
            CurrencyId = model.CurrencyId,
            ExchangeRate = model.ExchangeRate,
            DateExchangeStart = model.DateExchangeStart,
            DateExchangeEnd = model.DateExchangeEnd,
            Status = model.Status,
            Currency = model.Currency
        };
    }
}

// CurrencyExchangeRatesViewModel.cs

