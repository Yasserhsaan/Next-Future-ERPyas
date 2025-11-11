using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.PurchaseInvoices.Models;
using Next_Future_ERP.Features.PurchaseInvoices.Services;
using Next_Future_ERP.Features.Purchases.Services;
using Next_Future_ERP.Features.StoreReceipts.Services;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Warehouses.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.PurchaseInvoices.ViewModels
{
    public partial class PurchaseAPListViewModel : ObservableObject
    {
        private readonly IPurchaseAPService _service;
        private readonly ISuppliersService _suppliersService;

        public ObservableCollection<PurchaseAP> Items { get; } = new();
        public ObservableCollection<Supplier> Suppliers { get; } = new();

        [ObservableProperty] private PurchaseAP? selectedItem;
        [ObservableProperty] private string? searchText;
        [ObservableProperty] private Supplier? selectedSupplier;
        [ObservableProperty] private DateTime? fromDate;
        [ObservableProperty] private DateTime? toDate;
        [ObservableProperty] private string selectedDocType = "فاتورة مشتريات";
        [ObservableProperty] private bool canPost;
        [ObservableProperty] private bool canUnpost;
        [ObservableProperty] private bool isLoading;

        public IRelayCommand NewInvoiceCommand => new RelayCommand(OpenNewInvoiceDialog);
        public IRelayCommand NewReturnCommand => new RelayCommand(OpenNewReturnDialog);
        public IRelayCommand EditDialogCommand => new RelayCommand(OpenEditDialog, () => SelectedItem != null);
        public IRelayCommand DeleteCommand => new AsyncRelayCommand(DeleteAsync);
        public IRelayCommand RefreshCommand => new AsyncRelayCommand(LoadAsync);
        public IRelayCommand LoadCommand => new AsyncRelayCommand(LoadAsync);
        public IRelayCommand PostCommand => new AsyncRelayCommand(PostAsync, () => SelectedItem != null && SelectedItem.Status == 1);
        public IRelayCommand UnpostCommand => new AsyncRelayCommand(UnpostAsync, () => SelectedItem != null && SelectedItem.Status == 2);

        public PurchaseAPListViewModel(IPurchaseAPService service, ISuppliersService suppliersService)
        {
            _service = service;
            _suppliersService = suppliersService;
            _ = LoadAsync();
        }

        partial void OnSelectedItemChanged(PurchaseAP? oldValue, PurchaseAP? newValue)
        {
            (EditDialogCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (PostCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
            (UnpostCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
            
            // تحديث خصائص الأزرار
            CanPost = newValue != null && newValue.Status == 1;
            CanUnpost = newValue != null && newValue.Status == 2;
        }

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        partial void OnSelectedSupplierChanged(Supplier? oldValue, Supplier? newValue)
            => _ = LoadAsync();

        partial void OnFromDateChanged(DateTime? oldValue, DateTime? newValue)
            => _ = LoadAsync();

        partial void OnToDateChanged(DateTime? oldValue, DateTime? newValue)
            => _ = LoadAsync();

        partial void OnSelectedDocTypeChanged(string oldValue, string newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                
                // تحويل النص إلى كود
                string docTypeCode = SelectedDocType switch
                {
                    "فاتورة مشتريات" => "PI",
                    "مرتجع مشتريات" => "PR",
                    "الكل" => "",
                    _ => ""
                };

                var list = await _service.GetAllAsync(SearchText, SelectedSupplier?.SupplierID, FromDate, ToDate, docTypeCode);
                Items.Clear();
                foreach (var item in list)
                    Items.Add(item);

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

        private void OpenNewInvoiceDialog()
        {
            var vm = new PurchaseAPEditViewModel(
                _service,
                App.ServiceProvider.GetRequiredService<ISuppliersService>(),
                App.ServiceProvider.GetRequiredService<IItemsService>(),
                App.ServiceProvider.GetRequiredService<IUnitsService>(),
                App.ServiceProvider.GetRequiredService<IWarehouseService>(),
                App.ServiceProvider.GetRequiredService<IStoreReceiptsService>(),
                App.ServiceProvider.GetRequiredService<IPurchaseTxnsService>(),
                new PurchaseAP 
                { 
                    DocType = "PI",
                    DocDate = DateTime.Today, 
                    Status = 0,
                    CompanyId = 1,
                    BranchId = 1,
                    CurrencyId = 1,
                    ExchangeRate = 1
                });

            var win = new Views.PurchaseAPEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true)
                _ = LoadAsync();
        }

        private void OpenNewReturnDialog()
        {
            var vm = new PurchaseAPEditViewModel(
                _service,
                App.ServiceProvider.GetRequiredService<ISuppliersService>(),
                App.ServiceProvider.GetRequiredService<IItemsService>(),
                App.ServiceProvider.GetRequiredService<IUnitsService>(),
                App.ServiceProvider.GetRequiredService<IWarehouseService>(),
                App.ServiceProvider.GetRequiredService<IStoreReceiptsService>(),
                App.ServiceProvider.GetRequiredService<IPurchaseTxnsService>(),
                new PurchaseAP 
                { 
                    DocType = "PR",
                    DocDate = DateTime.Today, 
                    Status = 0,
                    CompanyId = 1,
                    BranchId = 1,
                    CurrencyId = 1,
                    ExchangeRate = 1
                });

            var win = new Views.PurchaseAPEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true)
                _ = LoadAsync();
        }

        private async void OpenEditDialog()
        {
            if (SelectedItem is null) return;

            try
            {
                var vm = await PurchaseAPEditViewModel.FromExisting(
                    _service,
                    App.ServiceProvider.GetRequiredService<ISuppliersService>(),
                    App.ServiceProvider.GetRequiredService<IItemsService>(),
                    App.ServiceProvider.GetRequiredService<IUnitsService>(),
                    App.ServiceProvider.GetRequiredService<IWarehouseService>(),
                    App.ServiceProvider.GetRequiredService<IStoreReceiptsService>(),
                    App.ServiceProvider.GetRequiredService<IPurchaseTxnsService>(),
                    SelectedItem.APId);

                var win = new Views.PurchaseAPEditWindow
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
            
            if (MessageBox.Show("هل تريد حذف المستند المحدد؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await _service.DeleteAsync(SelectedItem.APId);
                    await LoadAsync();
                    MessageBox.Show("تم حذف المستند بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في الحذف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task PostAsync()
        {
            if (SelectedItem is null) return;

            if (MessageBox.Show("هل تريد ترحيل المستند؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var result = await _service.PostAsync(SelectedItem.APId, 1); // TODO: استخدام معرف المستخدم الحقيقي
                    if (result)
                    {
                        await LoadAsync();
                        MessageBox.Show("تم ترحيل المستند بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في ترحيل المستند.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (MessageBox.Show("هل تريد عكس ترحيل المستند؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var result = await _service.UnpostAsync(SelectedItem.APId, 1); // TODO: استخدام معرف المستخدم الحقيقي
                    if (result)
                    {
                        await LoadAsync();
                        MessageBox.Show("تم عكس ترحيل المستند بنجاح.", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في عكس ترحيل المستند.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في عكس الترحيل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
