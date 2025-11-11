
﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Next_Future_ERP.Features.Accounts.Views;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class NextCurrenciesViewModel : ObservableObject
    {
        private readonly INextCurrencyService _service;

        public ObservableCollection<NextCurrency> Items { get; } = new();

        [ObservableProperty] 
        [NotifyCanExecuteChangedFor(nameof(EditDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private NextCurrency? selectedItem;
        
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

        public NextCurrenciesViewModel(INextCurrencyService service)
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

        partial void OnSelectedItemChanged(NextCurrency? oldValue, NextCurrency? newValue)
            => EditDialogCommand.NotifyCanExecuteChanged();

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            Items.Clear();
            var list = await _service.GetAllAsync();
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim();
                list = list.Where(x =>
                    (x.CurrencyNameAr ?? "").Contains(s)
                    || (x.CurrencyNameEn ?? "").Contains(s)
                    || (x.CurrencySymbol ?? "").Contains(s)
                ).ToList();
            }
            
            foreach (var item in list) Items.Add(item);
        }

        private void OpenNewDialog()
        {
            var vm = new CurrencyEditViewModel(_service, new NextCurrency());
            var win = new CurrencyEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            var vm = new CurrencyEditViewModel(_service, Clone(SelectedItem));
            var win = new CurrencyEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف العملة المحددة؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.CurrencyId);
                await LoadAsync();
            }
        }

        private static NextCurrency Clone(NextCurrency model) => new()
        {
            CurrencyId = model.CurrencyId,
            CompanyId = model.CompanyId,
            CurrencyNameAr = model.CurrencyNameAr,
            CurrencyNameEn = model.CurrencyNameEn,
            CurrencySymbol = model.CurrencySymbol,
            FractionUnit = model.FractionUnit,
            DecimalPlaces = model.DecimalPlaces,
            IsCompanyCurrency = model.IsCompanyCurrency,
            IsForeignCurrency = model.IsForeignCurrency,
            ExchangeRate = model.ExchangeRate,
            MinExchangeRate = model.MinExchangeRate,
            MaxExchangeRate = model.MaxExchangeRate,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }
}