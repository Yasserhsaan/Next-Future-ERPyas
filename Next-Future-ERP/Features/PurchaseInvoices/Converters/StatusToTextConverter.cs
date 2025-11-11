using System.Globalization;
using System.Windows.Data;

namespace Next_Future_ERP.Features.PurchaseInvoices.Converters
{
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte status)
            {
                return status switch
                {
                    0 => "مسودة",
                    1 => "محفوظ",
                    2 => "مرحل",
                    8 => "معكوس",
                    9 => "ملغي",
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
}
