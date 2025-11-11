using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Items.ViewModels
{
    public partial class ItemBatchesViewModel : ObservableObject
    {
        private readonly IItemBatchesService _service;

        [ObservableProperty]
        private int? currentItemId;
        
        partial void OnCurrentItemIdChanged(int? oldValue, int? newValue)
        {
            // تحميل البيانات بشكل آمن
            _ = LoadAsync();
        }

        [ObservableProperty]
        private ObservableCollection<ItemBatch> batches = new();

        [ObservableProperty]
        private ItemBatch? edit = new();

        public ItemBatchesViewModel(IItemBatchesService service)
        {
            _service = service;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                if (CurrentItemId == null || CurrentItemId <= 0) 
                {
                    await Application.Current.Dispatcher.InvokeAsync(() => Batches.Clear());
                    return;
                }
                
                var list = await _service.GetByItemAsync(CurrentItemId.Value);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Batches.Clear();
                    foreach (var b in list) Batches.Add(b);
                    if (Edit == null) New();
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        [RelayCommand]
        public void New()
        {
            Edit = new ItemBatch
            {
                ItemID = CurrentItemId ?? 0,
                BatchStatus = "A",
                IsActive = true,
                CurrentQuantity = 0m,
                ReservedQuantity = 0m
            };
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                if (Edit == null || CurrentItemId == null || CurrentItemId <= 0)
                {
                    MessageBox.Show("اختر صنفاً أولاً.");
                    return;
                }
                Edit.ItemID = CurrentItemId.Value;
                if (string.IsNullOrWhiteSpace(Edit.BatchNumber))
                {
                    MessageBox.Show("أدخل رقم الدفعة.");
                    return;
                }
                if (Edit.BatchStatus is not ("A" or "C" or "R")) Edit.BatchStatus = "A";

                if (Edit.BatchID == 0)
                    Edit.BatchID = await _service.AddAsync(Edit);
                else
                    await _service.UpdateAsync(Edit);

                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task DeleteAsync(ItemBatch row)
        {
            if (row == null) return;
            await _service.DeleteAsync(row.BatchID);
            await LoadAsync();
        }
    }
}


