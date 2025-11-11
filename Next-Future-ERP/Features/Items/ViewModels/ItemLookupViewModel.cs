using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Next_Future_ERP.Features.Items.ViewModels
{
    public partial class ItemLookupViewModel : ObservableObject
    {
        private readonly IItemsService _service;

        public ObservableCollection<Item> Items { get; } = new();

        [ObservableProperty] private string? searchText;
        [ObservableProperty] private Item? selected;

        public ItemLookupViewModel(IItemsService service)
        {
            _service = service;
        }

        public async Task LoadAsync()
        {
            var list = await _service.GetAllAsync(SearchText);
            Items.Clear();
            foreach (var i in list) Items.Add(i);
        }

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
        {
            // تحديث فوري عند الكتابة
            _ = LoadAsync();
        }

        public void PreselectById(int? itemId)
        {
            if (itemId is null || itemId <= 0) return;
            Selected = Items.FirstOrDefault(i => i.ItemID == itemId);
        }

        [RelayCommand]
        private async Task Search()
        {
            await LoadAsync();
        }

        [RelayCommand]
        private async Task Clear()
        {
            SearchText = null;
            await LoadAsync();
        }
    }
}
