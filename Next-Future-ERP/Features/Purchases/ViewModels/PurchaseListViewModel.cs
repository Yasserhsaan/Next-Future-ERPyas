// Features/Purchases/ViewModels/PurchaseListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Purchases.Models;
using Next_Future_ERP.Features.Purchases.Services;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Warehouses.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Purchases.ViewModels
{
    public partial class PurchaseListViewModel : ObservableObject
    {
        private readonly IPurchaseTxnsService _service;
        private readonly ISuppliersService _suppliersService;   // ✅

        private readonly char _txnType; // 'P' أو 'R'

        public ObservableCollection<PurchaseTxn> Items { get; } = new();
        public ObservableCollection<Supplier> Suppliers { get; } = new();  // ✅

        [ObservableProperty] private PurchaseTxn? selectedItem;
        [ObservableProperty] private string? searchText;
        [ObservableProperty] private bool isLoading;

        public IRelayCommand NewDialogCommand { get; }
        public IRelayCommand EditDialogCommand { get; }
        public IRelayCommand DeleteCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand LoadCommand { get; }

        // أوامر تغيير الحالة
        public IAsyncRelayCommand PostCommand { get; }
        public IAsyncRelayCommand ApproveCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }

        public PurchaseListViewModel(IPurchaseTxnsService service, ISuppliersService suppliersService, char txnType)
        {
            _service = service;
            _suppliersService = suppliersService;  // ✅
            _txnType = txnType;
            
            // تهيئة الأوامر
            NewDialogCommand = new RelayCommand(OpenNewDialog);
            EditDialogCommand = new RelayCommand(OpenEditDialog, () => 
            {
                var canExecute = SelectedItem != null;
                System.Diagnostics.Debug.WriteLine($"EditDialogCommand CanExecute: {canExecute}, SelectedItem: {SelectedItem?.TxnNumber}");
                return canExecute;
            });
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedItem != null);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            LoadCommand = new AsyncRelayCommand(LoadAsync);

            // تهيئة أوامر تغيير الحالة
            PostCommand = new AsyncRelayCommand(PostAsync, () => SelectedItem?.Status == 0);
            ApproveCommand = new AsyncRelayCommand(ApproveAsync, () => SelectedItem?.Status == 1);
            CancelCommand = new AsyncRelayCommand(CancelAsync, () => SelectedItem != null && SelectedItem.Status != 9);
            
            _ = LoadAsync();
        }

        partial void OnSelectedItemChanged(PurchaseTxn? oldValue, PurchaseTxn? newValue)
        {
            // تحديث حالة الأوامر التي تعتمد على SelectedItem
            (EditDialogCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (DeleteCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
            
            // تحديث حالة أوامر تغيير الحالة
            PostCommand.NotifyCanExecuteChanged();
            ApproveCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
        }

        partial void OnSearchTextChanged(string? oldValue, string? newValue)
            => _ = LoadAsync();

        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                
                // حفظ العنصر المحدد حالياً
                var currentSelectedId = SelectedItem?.TxnID;
                
                // حمّل المستندات
                var list = await _service.GetAllAsync(_txnType, SearchText);
                Items.Clear();
                foreach (var x in list) Items.Add(x);

                // إعادة تحديد العنصر إذا كان موجوداً
                if (currentSelectedId.HasValue)
                {
                    SelectedItem = Items.FirstOrDefault(x => x.TxnID == currentSelectedId.Value);
                }

                // حمّل الموردين (مرة واحدة عادة)
                if (Suppliers.Count == 0)
                {
                    var sup = await _suppliersService.GetAllAsync();
                    foreach (var s in sup.OrderBy(x => x.SupplierName)) Suppliers.Add(s);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadAsync: {ex.Message}");
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenNewDialog()
        {
            var vm = new PurchaseEditViewModel(
                _service,
                App.ServiceProvider.GetRequiredService<ISuppliersService>(),
                App.ServiceProvider.GetRequiredService<IItemsService>(),
                App.ServiceProvider.GetRequiredService<IUnitsService>(),
                _txnType,
                new PurchaseTxn { TxnType = _txnType, TxnDate = DateTime.Today, Status = 0 });

            var win = new Features.Purchases.Views.PurchaseEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true) _ = LoadAsync();
        }

        private async void OpenEditDialog()
        {
            System.Diagnostics.Debug.WriteLine($"OpenEditDialog called. SelectedItem: {SelectedItem?.TxnNumber}");
            
            if (SelectedItem is null) 
            {
                MessageBox.Show("لم يتم تحديد أي مستند للتعديل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Opening edit dialog for TxnID: {SelectedItem.TxnID}");
                
                var vm = await PurchaseEditViewModel.FromExisting(
                    _service,
                    App.ServiceProvider.GetRequiredService<ISuppliersService>(),
                    App.ServiceProvider.GetRequiredService<IItemsService>(),
                    App.ServiceProvider.GetRequiredService<IUnitsService>(),
                    SelectedItem.TxnID);

                var win = new Features.Purchases.Views.PurchaseEditWindow
                { DataContext = vm, Owner = Application.Current.MainWindow };

                System.Diagnostics.Debug.WriteLine("Showing dialog...");
                if (win.ShowDialog() == true)
                {
                    System.Diagnostics.Debug.WriteLine("Dialog closed with OK");
                    await LoadAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Dialog closed with Cancel");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in OpenEditDialog: {ex}");
                MessageBox.Show($"خطأ في فتح نافذة التعديل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private async Task DeleteAsync()
        {
            if (SelectedItem is null) return;
            if (MessageBox.Show("هل تريد حذف المستند المحدد؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(SelectedItem.TxnID);
                await LoadAsync();
            }
        }

        // دوال تغيير الحالة
        private async Task PostAsync()
        {
            if (SelectedItem is null) return;
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"PostAsync: Posting transaction {SelectedItem.TxnNumber}");
                
                var success = await _service.PostAsync(SelectedItem.TxnID);
                if (success)
                {
                    MessageBox.Show($"تم ترحيل المستند {SelectedItem.TxnNumber} بنجاح", "نجح الترحيل", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadAsync(); // إعادة تحميل القائمة لتحديث الحالة
                }
                else
                {
                    MessageBox.Show($"فشل في ترحيل المستند {SelectedItem.TxnNumber}", "خطأ في الترحيل", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PostAsync error: {ex.Message}");
                MessageBox.Show($"خطأ في ترحيل المستند: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ApproveAsync()
        {
            if (SelectedItem is null) return;
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"ApproveAsync: Approving transaction {SelectedItem.TxnNumber}");
                
                var success = await _service.ApproveAsync(SelectedItem.TxnID);
                if (success)
                {
                    MessageBox.Show($"تم اعتماد المستند {SelectedItem.TxnNumber} بنجاح", "نجح الاعتماد", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadAsync(); // إعادة تحميل القائمة لتحديث الحالة
                }
                else
                {
                    MessageBox.Show($"فشل في اعتماد المستند {SelectedItem.TxnNumber}", "خطأ في الاعتماد", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApproveAsync error: {ex.Message}");
                MessageBox.Show($"خطأ في اعتماد المستند: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CancelAsync()
        {
            if (SelectedItem is null) return;
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"CancelAsync: Cancelling transaction {SelectedItem.TxnNumber}");
                
                var result = MessageBox.Show($"هل تريد إلغاء المستند {SelectedItem.TxnNumber}؟", "تأكيد الإلغاء", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    var success = await _service.CancelAsync(SelectedItem.TxnID);
                    if (success)
                    {
                        MessageBox.Show($"تم إلغاء المستند {SelectedItem.TxnNumber} بنجاح", "نجح الإلغاء", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadAsync(); // إعادة تحميل القائمة لتحديث الحالة
                    }
                    else
                    {
                        MessageBox.Show($"فشل في إلغاء المستند {SelectedItem.TxnNumber}", "خطأ في الإلغاء", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CancelAsync error: {ex.Message}");
                MessageBox.Show($"خطأ في إلغاء المستند: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
