using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using System;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.ViewModels
{
    public partial class ItemCostsViewModel : ObservableObject
    {
        private readonly IItemCostsService _service;
        public ItemCostsViewModel(IItemCostsService service)
        {
            _service = service;
        }

        [ObservableProperty] private ItemCost edit = new() { CostMethod = "A" };
        [ObservableProperty] private int? itemId;

        partial void OnItemIdChanged(int? oldValue, int? newValue)
        {
            // تحميل البيانات بشكل آمن
            _ = LoadAsync();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            var id = ItemId ?? 0;
            if (id <= 0) return;
            
            var row = await _service.GetByItemIdAsync(id);
            if (row != null) Edit = row; else Edit = new ItemCost { ItemID = id, CostMethod = "A" };
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            if ((ItemId ?? 0) <= 0) return;
            Edit.ItemID = ItemId!.Value;
            if (string.IsNullOrWhiteSpace(Edit.CostMethod)) Edit.CostMethod = "A";
            Edit.CostMethod = Edit.CostMethod.Trim().Substring(0, 1).ToUpperInvariant();
            if (Edit.CostMethod != "L" && Edit.CostMethod != "A" && Edit.CostMethod != "F" && Edit.CostMethod != "M")
                Edit.CostMethod = "A";

            // تطبيع القيم غير القابلة للإلغاء
            if (Edit.StandardCost < 0) Edit.StandardCost = 0;
            if (Edit.LastPurchaseCost < 0) Edit.LastPurchaseCost = 0;
            if (Edit.MovingAverageCost < 0) Edit.MovingAverageCost = 0;
            if (Edit.FIFOCost < 0) Edit.FIFOCost = 0;
            Edit.LastUpdate = DateTime.Now;
            await _service.UpsertAsync(Edit);
            await LoadAsync();
        }
    }
}


