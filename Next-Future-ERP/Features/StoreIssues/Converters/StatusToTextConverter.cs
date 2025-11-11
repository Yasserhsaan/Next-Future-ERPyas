using System.Globalization;
using System.Windows.Data;

namespace Next_Future_ERP.Features.StoreIssues.Converters
{
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "نشط" : "غير نشط";
            }

            return "غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text == "نشط";
            }

            return false;
        }
    }
}
