using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Items.Views
{
    public partial class ItemSearchWindowViewModel : ObservableObject
    {
        private readonly IItemsService _itemsService;

        [ObservableProperty] private ObservableCollection<Item> items = new();
        [ObservableProperty] private Item? selectedItem;
        [ObservableProperty] private string? searchText;

        public ItemSearchWindowViewModel(IItemsService itemsService)
        {
            _itemsService = itemsService;
            LoadItemsCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            SelectCommand = new RelayCommand(SelectItem, () => SelectedItem != null);
        }

        public IAsyncRelayCommand LoadItemsCommand { get; }
        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand SelectCommand { get; }

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
        {
            _ = SearchAsync();
        }

        private async Task LoadItemsAsync()
        {
            var allItems = await _itemsService.GetAllAsync();
            Items.Clear();
            foreach (var item in allItems.OrderBy(x => x.ItemName))
            {
                Items.Add(item);
            }
        }

        private async Task SearchAsync()
        {
            var filteredItems = await _itemsService.GetAllAsync(SearchText);
            Items.Clear();
            foreach (var item in filteredItems.OrderBy(x => x.ItemName))
            {
                Items.Add(item);
            }
        }

        private void SelectItem()
        {
            if (SelectedItem != null)
            {
                // إغلاق النافذة مع نتيجة إيجابية
                var window = Application.Current.Windows.OfType<ItemSearchWindow>().FirstOrDefault();
                window!.DialogResult = true;
                window.Close();
            }
        }
    }
}
