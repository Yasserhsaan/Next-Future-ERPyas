using Next_Future_ERP.Features.Items.ViewModels;
using Next_Future_ERP.Features.Items.Views;
using System.Windows;
using Wpf.Ui.Controls;
using System;

namespace Next_Future_ERP.Features.Items.Views
{
    /// <summary>
    /// Interaction logic for ItemEditWindow.xaml
    /// </summary>
    public partial class ItemEditWindow : FluentWindow
    {
        public ItemEditWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("خطأ في تهيئة النافذة", ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is ItemEditViewModel vm)
                {
                    vm.CloseRequested += (s, result) =>
                    {
                        try
                        {
                            DialogResult = result;
                            Close();
                        }
                        catch (Exception ex)
                        {
                            ShowErrorDialog("خطأ في إغلاق النافذة", ex);
                        }
                    };
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ItemEditWindow: DataContext is not ItemEditViewModel");
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("خطأ في تحميل النافذة", ex);
            }
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (DataContext is ItemEditViewModel vm && sender is System.Windows.Controls.TabControl tabControl)
                {
                    if (tabControl.SelectedItem is System.Windows.Controls.TabItem selectedTab)
                    {
                        // الحصول على نص التبويب المختار
                        string tabText = GetTabText(selectedTab);
                        
                        if (string.IsNullOrEmpty(tabText))
                        {
                            System.Diagnostics.Debug.WriteLine("TabControl_SelectionChanged: Empty tab text");
                            return;
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"TabControl_SelectionChanged: Switching to tab '{tabText}'");
                        
                        // تحميل البيانات حسب التبويب المختار
                        switch (tabText)
                        {
                            case "الأسعار":
                                LoadTabSafely(() => vm.LoadPricesTabAsync(), "تحميل بيانات الأسعار");
                                break;
                                
                            case "الوحدات والباركود":
                                LoadTabSafely(() => vm.LoadUnitsTabAsync(), "تحميل بيانات الوحدات");
                                break;
                                
                            case "التكاليف":
                                LoadTabSafely(() => vm.LoadCostsTabAsync(), "تحميل بيانات التكاليف");
                                break;
                                
                            case "الموردون":
                                LoadTabSafely(() => vm.LoadSuppliersTabAsync(), "تحميل بيانات الموردين");
                                break;
                                
                            case "الدفعات":
                                LoadTabSafely(() => vm.LoadBatchesTabAsync(), "تحميل بيانات الدفعات");
                                break;
                                
                            case "المكونات":
                                LoadTabSafely(() => vm.LoadComponentsTabAsync(), "تحميل بيانات المكونات");
                                break;
                                
                            case "الأرصدة المخزنية":
                                LoadTabSafely(() => vm.LoadInventoryTabAsync(), "تحميل بيانات الأرصدة");
                                break;
                                
                            default:
                                System.Diagnostics.Debug.WriteLine($"TabControl_SelectionChanged: Unknown tab '{tabText}'");
                                break;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("TabControl_SelectionChanged: SelectedItem is not TabItem");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("TabControl_SelectionChanged: DataContext is not ItemEditViewModel or sender is not TabControl");
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("خطأ في تغيير التبويب", ex);
            }
        }
        
        private string GetTabText(System.Windows.Controls.TabItem tabItem)
        {
            try
            {
                if (tabItem.Header is System.Windows.Controls.StackPanel headerPanel && 
                    headerPanel.Children.Count > 1 && 
                    headerPanel.Children[1] is System.Windows.Controls.TextBlock textBlock)
                {
                    return textBlock.Text;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTabText error: {ex.Message}");
            }
            return string.Empty;
        }

        private async void LoadTabSafely(Func<Task> loadAction, string operationName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadTabSafely: Starting {operationName}");
                
                if (loadAction == null)
                {
                    System.Diagnostics.Debug.WriteLine($"LoadTabSafely: loadAction is null for {operationName}");
                    return;
                }
                
                await loadAction();
                
                System.Diagnostics.Debug.WriteLine($"LoadTabSafely: Completed {operationName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTabSafely: Error in {operationName}: {ex.Message}");
                ShowErrorDialog($"خطأ في {operationName}", ex);
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
    }
}
