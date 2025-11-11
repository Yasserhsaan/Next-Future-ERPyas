using System.Globalization;
using System.Windows.Data;

namespace Next_Future_ERP.Features.PurchaseInvoices.Converters
{
    public class DocTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string docType)
            {
                return docType switch
                {
                    "PI" => "فاتورة مشتريات",
                    "PR" => "مرتجع مشتريات",
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
