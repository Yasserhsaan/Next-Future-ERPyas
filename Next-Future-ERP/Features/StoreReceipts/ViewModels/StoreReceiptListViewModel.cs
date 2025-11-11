using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.StoreReceipts.Models;
using Next_Future_ERP.Features.StoreReceipts.Services;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Warehouses.Services;
using Next_Future_ERP.Features.Purchases.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.StoreReceipts.ViewModels
{
    public partial class StoreReceiptListViewModel : ObservableObject
    {
        private readonly IStoreReceiptsService _service;
        private readonly ISuppliersService _suppliersService;

        public ObservableCollection<StoreReceipt> Items { get; } = new();
        public ObservableCollection<Supplier> Suppliers { get; } = new();

        [ObservableProperty] private StoreReceipt? selectedItem;
        [ObservableProperty] private string? searchText;
        [ObservableProperty] private Supplier? selectedSupplier;
        [ObservableProperty] private DateTime? fromDate;
        [ObservableProperty] private DateTime? toDate;
        [ObservableProperty] private bool canPost;
        [ObservableProperty] private bool canUnpost;
        [ObservableProperty] private bool canApprove;
        [ObservableProperty] private bool isLoading;

        public IRelayCommand NewDialogCommand => new RelayCommand(OpenNewDialog);
        public IRelayCommand EditDialogCommand => new RelayCommand(OpenEditDialog, () => SelectedItem != null);
        public IRelayCommand DeleteCommand => new AsyncRelayCommand(DeleteAsync);
        public IRelayCommand RefreshCommand => new AsyncRelayCommand(LoadAsync);
        public IRelayCommand LoadCommand => new AsyncRelayCommand(LoadAsync);
        public IRelayCommand PostCommand => new AsyncRelayCommand(PostAsync, () => SelectedItem != null);
        public IRelayCommand UnpostCommand => new AsyncRelayCommand(UnpostAsync, () => SelectedItem != null);
        public IRelayCommand ApproveCommand => new AsyncRelayCommand(ApproveAsync, () => SelectedItem != null);

        public StoreReceiptListViewModel(IStoreReceiptsService service, ISuppliersService suppliersService)
        {
            _service = service;
            _suppliersService = suppliersService;
            _ = LoadAsync();
        }

        partial void OnSelectedItemChanged(StoreReceipt? oldValue, StoreReceipt? newValue)
        {
            (EditDialogCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (PostCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
            (UnpostCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
            (ApproveCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
            
            // تحديث خصائص الأزرار
            CanPost = newValue != null;
            CanUnpost = newValue != null;
            CanApprove = newValue != null;
            
            // إشعار تغيير حالة الأزرار
            OnPropertyChanged(nameof(PostCommand));
            OnPropertyChanged(nameof(UnpostCommand));
            OnPropertyChanged(nameof(ApproveCommand));
            
            // تصحيح للتأكد من أن التغيير يحدث
            System.Diagnostics.Debug.WriteLine($"SelectedItem changed: {newValue?.ReceiptNumber}, Status: {newValue?.Status}");
        }

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        partial void OnSelectedSupplierChanged(Supplier? oldValue, Supplier? newValue)
            => _ = LoadAsync();

        partial void OnFromDateChanged(DateTime? oldValue, DateTime? newValue)
            => _ = LoadAsync();

        partial void OnToDateChanged(DateTime? oldValue, DateTime? newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                
                var list = await _service.GetAllAsync(SearchText, SelectedSupplier?.SupplierID, FromDate, ToDate);
                Items.Clear();
                foreach (var item in list)
                {
                    System.Diagnostics.Debug.WriteLine($"Adding item: {item.ReceiptNumber}, Status: {item.Status}, StatusText: {item.StatusText}");
                    Items.Add(item);
                }

                // تحميل الموردين (مرة واحدة)
                if (Suppliers.Count == 0)
                {
                    var suppliers = await _suppliersService.GetAllAsync();
                    foreach (var supplier in suppliers.OrderBy(x => x.SupplierName))
                        Suppliers.Add(supplier);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenNewDialog()
        {
            var vm = new StoreReceiptEditViewModel(
                _service,
                App.ServiceProvider.GetRequiredService<ISuppliersService>(),
                App.ServiceProvider.GetRequiredService<IItemsService>(),
                App.ServiceProvider.GetRequiredService<IUnitsService>(),
                App.ServiceProvider.GetRequiredService<IWarehouseService>(),
                App.ServiceProvider.GetRequiredService<IPurchaseTxnsService>(),
                new StoreReceipt 
                { 
                    ReceiptDate = DateTime.Today, 
                    Status = 0,
                    CompanyId = 1,
                    BranchId = 1,
                    CurrencyId = 1,
                    ExchangeRate = 1
                });

            var win = new Views.StoreReceiptEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true)
                _ = LoadAsync();
        }

        private async void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            try
            {
                var vm = await StoreReceiptEditViewModel.FromExisting(
                    _service,
                    App.ServiceProvider.GetRequiredService<ISuppliersService>(),
                    App.ServiceProvider.GetRequiredService<IItemsService>(),
                    App.ServiceProvider.GetRequiredService<IUnitsService>(),
                    App.ServiceProvider.GetRequiredService<IWarehouseService>(),
                    App.ServiceProvider.GetRequiredService<IPurchaseTxnsService>(),
                    SelectedItem.ReceiptId);

                var win = new Views.StoreReceiptEditWindow
                { DataContext = vm, Owner = Application.Current.MainWindow };

                if (win.ShowDialog() == true)
                    await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح المستند: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            
            if (MessageBox.Show("هل تريد حذف سند الاستلام المحدد؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await _service.DeleteAsync(SelectedItem.ReceiptId);
                    await LoadAsync();
                    MessageBox.Show("تم حذف سند الاستلام بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في الحذف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task PostAsync()
        {
            System.Diagnostics.Debug.WriteLine($"PostAsync called - SelectedItem: {SelectedItem?.ReceiptNumber}, Status: {SelectedItem?.Status}");
            
            if (SelectedItem is null) 
            {
                System.Diagnostics.Debug.WriteLine("SelectedItem is null");
                return;
            }

            // التحقق من حالة المستند
            if (SelectedItem.Status != 0)
            {
                System.Diagnostics.Debug.WriteLine($"Status is {SelectedItem.Status}, not 0");
                MessageBox.Show("لا يمكن ترحيل سند الاستلام. يجب أن يكون في حالة مسودة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("هل تريد ترحيل سند الاستلام؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var result = await _service.PostAsync(SelectedItem.ReceiptId, 1); // TODO: استخدام معرف المستخدم الحقيقي
                    if (result)
                    {
                        await LoadAsync();
                        MessageBox.Show("تم ترحيل سند الاستلام بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في ترحيل سند الاستلام.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في الترحيل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task UnpostAsync()
        {
            if (SelectedItem is null) return;

            // التحقق من حالة المستند
            if (SelectedItem.Status != 1)
            {
                MessageBox.Show("لا يمكن عكس ترحيل سند الاستلام. يجب أن يكون في حالة مرحل.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("هل تريد عكس ترحيل سند الاستلام؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var result = await _service.UnpostAsync(SelectedItem.ReceiptId, 1); // TODO: استخدام معرف المستخدم الحقيقي
                    if (result)
                    {
                        await LoadAsync();
                        MessageBox.Show("تم عكس ترحيل سند الاستلام بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في عكس ترحيل سند الاستلام.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في عكس الترحيل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ApproveAsync()
        {
            if (SelectedItem is null) return;

            // التحقق من حالة المستند
            if (SelectedItem.Status != 1)
            {
                MessageBox.Show("لا يمكن اعتماد سند الاستلام. يجب أن يكون في حالة مرحل.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("هل تريد اعتماد سند الاستلام؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var result = await _service.ApproveAsync(SelectedItem.ReceiptId, 1); // TODO: استخدام معرف المستخدم الحقيقي
                    if (result)
                    {
                        await LoadAsync();
                        MessageBox.Show("تم اعتماد سند الاستلام بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في اعتماد سند الاستلام.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في الاعتماد: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
