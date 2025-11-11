using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Inventory.Models;
using Next_Future_ERP.Features.Inventory.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Inventory.ViewModels
{
    public partial class InventoryOpeningBrowseViewModel : ObservableObject
    {
        private readonly IInventoryOpeningService _service;

        [ObservableProperty]
        private ObservableCollection<InventoryOpeningHeader> headers = new();

        [ObservableProperty]
        private InventoryOpeningHeader? selectedHeader;

        [ObservableProperty]
        private bool isLoading;

        public InventoryOpeningBrowseViewModel(IInventoryOpeningService service)
        {
            _service = service;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                Headers.Clear();
                // عرض جميع الحالات؛ سيتم منع اختيار المعتمد لاحقاً في الواجهة
                var items = await _service.GetHeadersAsync(companyId: 1, branchId: 1, status: null);
                foreach (var h in items.OrderByDescending(h => h.DocDate).ThenByDescending(h => h.DocID))
                    Headers.Add(h);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}


