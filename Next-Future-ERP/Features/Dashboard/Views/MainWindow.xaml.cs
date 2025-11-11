using Next_Future_ERP.Features.Dashboard.ViewModels;
using Next_Future_ERP.Features.Dashboard.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Wpf.Ui.Controls;
using Border = System.Windows.Controls.Border;
using CustomTabItem = Next_Future_ERP.Features.Dashboard.Models.TabItem;

namespace Next_Future_ERP.Features.Dashboard.Views
{
    public partial class MainWindow : FluentWindow
    {

        public MainWindow(MainWindowModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        // النقر على هيدر التبويب يفعّل SelectTab
        private void TabHeader_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is ContentPresenter cp && cp.Content is CustomTabItem tab)
            {
                if (DataContext is MainWindowModel vm)
                {
                    vm.SelectTabCommand.Execute(tab);
                }
            }
        }

            // النقر على عنصر من القائمة يفتح تبويب أو يوسع/يطوي العنصر الأب
            private void MenuBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                if (sender is Border border && border.Tag is MenuItemVm menuItem)
                {
                    if (DataContext is MainWindowModel vm)
                    {
                        // إذا كان العنصر يحتوي على عناصر فرعية، نفتح/نطوي العناصر الفرعية
                        if (menuItem.Children.Count > 0)
                        {
                            menuItem.IsExpanded = !menuItem.IsExpanded;
                            e.Handled = true;
                        }
                        // إذا كان العنصر يحتوي على PageType، نفتح تبويب ونحدده
                        else if (menuItem.PageType != null)
                        {
                            // إلغاء تحديد جميع العناصر
                            ClearAllSelections(vm.Menu);
                            
                            // تحديد العنصر الحالي
                            menuItem.IsSelected = true;
                            
                            vm.OpenFromMenuCommand.Execute(menuItem);
                            e.Handled = true;
                        }
                    }
                }
            }
            
            // إلغاء تحديد جميع عناصر القائمة
            private void ClearAllSelections(System.Collections.ObjectModel.ObservableCollection<MenuItemVm> menuItems)
            {
                foreach (var item in menuItems)
                {
                    item.IsSelected = false;
                    if (item.Children.Count > 0)
                    {
                        ClearAllSelections(item.Children);
                    }
                }
            }
    }

    // Converter للتحقق من وجود SelectedTab
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter لتحويل العدد إلى Visibility
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
