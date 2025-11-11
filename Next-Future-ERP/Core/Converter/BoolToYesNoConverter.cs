using System;
using System.Globalization;
using System.Windows.Data;

namespace Next_Future_ERP.Core.Converter
{
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "نعم" : "لا";
            }
            return "غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue.Equals("نعم", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
} 