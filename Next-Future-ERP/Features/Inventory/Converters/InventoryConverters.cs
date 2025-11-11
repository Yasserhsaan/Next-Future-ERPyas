using Next_Future_ERP.Features.Inventory.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Next_Future_ERP.Features.Inventory.Converters
{
    /// <summary>
    /// محول حالة المستند إلى نص
    /// </summary>
    public class InventoryStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InventoryOpeningStatus status)
            {
                return status switch
                {
                    InventoryOpeningStatus.Draft => "مسودة",
                    InventoryOpeningStatus.Approved => "معتمد",
                    _ => "غير محدد"
                };
            }
            return "غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// محول حالة المستند إلى لون
    /// </summary>
    public class InventoryStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InventoryOpeningStatus status)
            {
                return status switch
                {
                    InventoryOpeningStatus.Draft => "#FFA500", // برتقالي
                    InventoryOpeningStatus.Approved => "#008000", // أخضر
                    _ => "#808080" // رمادي
                };
            }
            return "#808080";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// محول طريقة الإدخال إلى نص
    /// </summary>
    public class EntryMethodToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EntryMethod method)
            {
                return method switch
                {
                    EntryMethod.Manual => "يدوي",
                    EntryMethod.Auto => "تلقائي",
                    _ => "غير محدد"
                };
            }
            return "غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text switch
                {
                    "يدوي" => EntryMethod.Manual,
                    "تلقائي" => EntryMethod.Auto,
                    _ => EntryMethod.Manual
                };
            }
            return EntryMethod.Manual;
        }
    }

    /// <summary>
    /// محول طريقة العرض إلى نص
    /// </summary>
    public class ViewModeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ViewMode mode)
            {
                return mode switch
                {
                    ViewMode.ByItem => "حسب الصنف",
                    ViewMode.ByWarehouse => "حسب المخزن",
                    _ => "غير محدد"
                };
            }
            return "غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text switch
                {
                    "حسب الصنف" => ViewMode.ByItem,
                    "حسب المخزن" => ViewMode.ByWarehouse,
                    _ => ViewMode.ByItem
                };
            }
            return ViewMode.ByItem;
        }
    }

    /// <summary>
    /// محول طريقة التكاليف إلى نص
    /// </summary>
    public class CostMethodToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CostMethod method)
            {
                return method switch
                {
                    CostMethod.WeightedAverage => "المتوسط المرجح",
                    CostMethod.FIFO => "الوارد أولاً الصادر أولاً",
                    _ => "غير محدد"
                };
            }
            return "غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text switch
                {
                    "المتوسط المرجح" => CostMethod.WeightedAverage,
                    "الوارد أولاً الصادر أولاً" => CostMethod.FIFO,
                    _ => CostMethod.WeightedAverage
                };
            }
            return CostMethod.WeightedAverage;
        }
    }

    /// <summary>
    /// محول نطاق المتوسط المرجح إلى نص
    /// </summary>
    public class WeightedAvgScopeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WeightedAvgScope scope)
            {
                return scope switch
                {
                    WeightedAvgScope.ByItem => "حسب الصنف",
                    WeightedAvgScope.ByItemWarehouse => "حسب الصنف والمخزن",
                    _ => "غير محدد"
                };
            }
            return "غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text switch
                {
                    "حسب الصنف" => WeightedAvgScope.ByItem,
                    "حسب الصنف والمخزن" => WeightedAvgScope.ByItemWarehouse,
                    _ => WeightedAvgScope.ByItem
                };
            }
            return WeightedAvgScope.ByItem;
        }
    }

    /// <summary>
    /// محول القيمة المالية إلى نص مع تنسيق العملة
    /// </summary>
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                return amount.ToString("N2", culture) + " ر.س"; // أو العملة المحددة
            }
            if (value is double doubleAmount)
            {
                return doubleAmount.ToString("N2", culture) + " ر.س";
            }
            return "0.00 ر.س";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                var cleanText = text.Replace("ر.س", "").Replace(",", "").Trim();
                if (decimal.TryParse(cleanText, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }

    /// <summary>
    /// محول الكمية إلى نص مع تنسيق الأرقام
    /// </summary>
    public class QuantityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal qty)
            {
                return qty.ToString("N3", culture);
            }
            if (value is double doubleQty)
            {
                return doubleQty.ToString("N3", culture);
            }
            return "0.000";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                var cleanText = text.Replace(",", "").Trim();
                if (decimal.TryParse(cleanText, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }

    /// <summary>
    /// محول Boolean إلى Visibility
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// محول Boolean معكوس إلى Visibility
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Collapsed;
            }
            return true;
        }
    }

    /// <summary>
    /// محول التاريخ إلى نص مختصر
    /// </summary>
    public class DateToShortStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.ToString("dd/MM/yyyy", culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && DateTime.TryParse(text, out DateTime result))
            {
                return result;
            }
            return DateTime.Today;
        }
    }

    /// <summary>
    /// محول لإظهار/إخفاء الحقول بناءً على إعدادات الترويسة
    /// </summary>
    public class HeaderSettingToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool setting && parameter is string fieldName)
            {
                // يمكن توسيع هذا المحول للتعامل مع حقول مختلفة
                return setting ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
