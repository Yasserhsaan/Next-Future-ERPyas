using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Suppliers.ViewModels
{
    public partial class SupplierLookupViewModel : ObservableObject
    {
        private readonly ISuppliersService _service;
        private System.Threading.Timer? _searchTimer;

        [ObservableProperty]
        private ObservableCollection<Supplier> items = new();

        [ObservableProperty]
        private Supplier? selected;

        [ObservableProperty]
        private string? search;

        public SupplierLookupViewModel(ISuppliersService service)
        {
            _service = service;
            _ = LoadAsync();
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            var list = await _service.GetAllAsync(Search);
            Items = new ObservableCollection<Supplier>(list);
        }

        partial void OnSearchChanged(string? value)
        {
            // إلغاء البحث السابق
            _searchTimer?.Dispose();
            
            // تأخير البحث لمدة 300ms لتجنب البحث المفرط
            _searchTimer = new System.Threading.Timer(async _ =>
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await LoadAsync();
                });
            }, null, 300, System.Threading.Timeout.Infinite);
        }
    }
}


