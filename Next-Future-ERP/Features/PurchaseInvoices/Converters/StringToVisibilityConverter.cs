using System.Globalization;
using System.Windows.Data;

namespace Next_Future_ERP.Features.PurchaseInvoices.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && parameter is string parameterValue)
            {
                return stringValue == parameterValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter is string parameterValue)
            {
                return parameterValue;
            }
            return null;
        }
    }
}
