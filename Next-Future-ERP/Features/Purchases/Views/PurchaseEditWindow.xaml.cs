using Next_Future_ERP.Features.Purchases.Models;
using Next_Future_ERP.Features.Purchases.ViewModels;
using Next_Future_ERP.Features.Suppliers.Models;
using System.Windows;
using System.Windows.Controls;
using System;

namespace Next_Future_ERP.Features.Purchases.Views
{
    public partial class PurchaseEditWindow : Window
    {
        public PurchaseEditWindow()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PurchaseEditWindow constructor called");
                InitializeComponent();

                Loaded += async (_, __) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("PurchaseEditWindow Loaded event fired");
                        if (DataContext is Next_Future_ERP.Features.Purchases.ViewModels.PurchaseEditViewModel vm)
                        {
                            System.Diagnostics.Debug.WriteLine("ViewModel found, setting up CloseRequested event");
                            vm.CloseRequested += (s, ok) => 
                            {
                                try
                                {
                                    DialogResult = ok;
                                    Close();
                                }
                                catch (Exception ex)
                                {
                                    ShowErrorDialog("خطأ في إغلاق النافذة", ex);
                                }
                            };
                            
                            System.Diagnostics.Debug.WriteLine("Calling InitializeAsync...");
                            await InitializeViewModelSafely(vm);
                            System.Diagnostics.Debug.WriteLine("InitializeAsync completed");
                            
                            // تحميل المورد المحدد في SupplierSearchBox
                            LoadSelectedSupplier(vm);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No ViewModel found in DataContext");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowErrorDialog("خطأ في تحميل النافذة", ex);
                    }
                };
            }
            catch (Exception ex)
            {
                ShowErrorDialog("خطأ في تهيئة النافذة", ex);
            }
        }


        private void DetailsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                // احصل على الـ ViewModel
                if (DataContext is not PurchaseEditViewModel vm) 
                {
                    System.Diagnostics.Debug.WriteLine("DetailsGrid_CellEditEnding: DataContext is not PurchaseEditViewModel");
                    return;
                }

                // نطبق فقط عند تعديل الكمية في المرتجع
                if (!vm.IsReturn || e.Column.Header.ToString() != "الكمية") 
                {
                    System.Diagnostics.Debug.WriteLine($"DetailsGrid_CellEditEnding: Not applicable - IsReturn: {vm.IsReturn}, Column: {e.Column.Header}");
                    return;
                }

                if (e.Row.Item is PurchaseTxnDetail detail)
                {
                    System.Diagnostics.Debug.WriteLine($"DetailsGrid_CellEditEnding: Editing quantity for item {detail.ItemID}");
                    
                    // البحث عن الكمية الأصلية في أمر الشراء
                    var parentQty = vm.SelectedParentOrder?.Details
                        .FirstOrDefault(d => d.ItemID == detail.ItemID)?.Quantity ?? 0;

                    System.Diagnostics.Debug.WriteLine($"DetailsGrid_CellEditEnding: Parent quantity: {parentQty}, Current quantity: {detail.Quantity}");

                    if (detail.Quantity > parentQty)
                    {
                        // ضبط القيمة على الحد الأقصى
                        detail.Quantity = parentQty;

                        ShowWarningDialog(
                            $"الكمية لا يمكن أن تتجاوز كمية أمر الشراء الأصلي ({parentQty}).",
                            "تحذير");
                    }

                    // إعادة حساب الإجماليات
                    vm.RecalcTotals();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DetailsGrid_CellEditEnding: Row.Item is not PurchaseTxnDetail");
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("خطأ في تعديل الخلية", ex);
            }
        }

        private async Task InitializeViewModelSafely(PurchaseEditViewModel vm)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("InitializeViewModelSafely: Starting initialization");
                await vm.InitializeAsync();
                System.Diagnostics.Debug.WriteLine("InitializeViewModelSafely: Initialization completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeViewModelSafely: Error during initialization: {ex.Message}");
                ShowErrorDialog("خطأ في تهيئة البيانات", ex);
            }
        }

        private void ShowErrorDialog(string title, Exception ex)
        {
            try
            {
                // استخدام Dispatcher لتجنب مشاكل التزامن
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        var errorMessage = $"{title}\n\n" +
                                         $"الخطأ: {ex.Message}\n\n" +
                                         $"التفاصيل التقنية:\n{ex.StackTrace}";
                        
                        System.Windows.MessageBox.Show(errorMessage, "خطأ في النظام", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                    catch (Exception messageBoxEx)
                    {
                        // في حالة فشل MessageBox، استخدام Debug
                        System.Diagnostics.Debug.WriteLine($"MessageBox error: {messageBoxEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Original error: {ex.Message}");
                        
                        // محاولة عرض رسالة بسيطة
                        try
                        {
                            System.Windows.MessageBox.Show($"خطأ: {ex.Message}", "خطأ", 
                                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        }
                        catch
                        {
                            // آخر محاولة - استخدام Console فقط
                            System.Diagnostics.Debug.WriteLine($"Critical error: {ex.Message}");
                        }
                    }
                }));
            }
            catch (Exception dispatcherEx)
            {
                // في حالة فشل Dispatcher، استخدام Console
                System.Diagnostics.Debug.WriteLine($"Dispatcher error: {dispatcherEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Original error: {ex.Message}");
                
                // محاولة عرض رسالة بسيطة بدون Dispatcher
                try
                {
                    System.Windows.MessageBox.Show($"خطأ: {ex.Message}", "خطأ", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
                catch
                {
                    // آخر محاولة - استخدام Console فقط
                    System.Diagnostics.Debug.WriteLine($"Critical error: {ex.Message}");
                }
            }
        }

        private void ShowWarningDialog(string message, string title)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        System.Windows.MessageBox.Show(message, title, 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning dialog error: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Warning message: {message}");
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning dialog dispatcher error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Warning message: {message}");
            }
        }

        // معالج حدث اختيار المورد من SupplierSearchBox
        private void SupplierSearchBox_SupplierSelected(object sender, Supplier selectedSupplier)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"SupplierSearchBox_SupplierSelected: Selected supplier {selectedSupplier.SupplierName} (ID: {selectedSupplier.SupplierID})");
                
                if (DataContext is PurchaseEditViewModel vm)
                {
                    vm.Model.SupplierID = selectedSupplier.SupplierID;
                    System.Diagnostics.Debug.WriteLine($"Updated Model.SupplierID to {vm.Model.SupplierID}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SupplierSearchBox_SupplierSelected error: {ex.Message}");
                ShowErrorDialog("خطأ في اختيار المورد", ex);
            }
        }

        // تحميل المورد المحدد في SupplierSearchBox
        private void LoadSelectedSupplier(PurchaseEditViewModel vm)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadSelectedSupplier: Loading selected supplier");
                
                var selectedSupplier = vm.GetSelectedSupplier();
                if (selectedSupplier != null)
                {
                    System.Diagnostics.Debug.WriteLine($"LoadSelectedSupplier: Setting supplier {selectedSupplier.SupplierName} (ID: {selectedSupplier.SupplierID})");
                    SupplierSearchBox.SetSupplier(selectedSupplier);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("LoadSelectedSupplier: No supplier selected");
                    SupplierSearchBox.Clear();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadSelectedSupplier error: {ex.Message}");
                ShowErrorDialog("خطأ في تحميل المورد المحدد", ex);
            }
        }

    }
}
