using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Next_Future_ERP.Features.Dashboard.Converters
{
    public class TypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "PO" => "📋",
                "GRN" => "📦",
                "PI" => "🧾",
                "PR" => "↩️",
                _ => "📄"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AlertTypeToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hex = value?.ToString() switch
            {
                "Warning" => "#FEF3C7", // أصفر فاتح
                "Error" => "#FEE2E2",   // أحمر فاتح
                "Info" => "#DBEAFE",    // أزرق فاتح
                "Success" => "#D1FAE5", // أخضر فاتح
                _ => "#F9FAFB"           // رمادي فاتح
            };
            return (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AlertTypeToBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hex = value?.ToString() switch
            {
                "Warning" => "#F59E0B", // أصفر
                "Error" => "#EF4444",   // أحمر
                "Info" => "#3B82F6",    // أزرق
                "Success" => "#10B981", // أخضر
                _ => "#E5E7EB"           // رمادي
            };
            return (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
