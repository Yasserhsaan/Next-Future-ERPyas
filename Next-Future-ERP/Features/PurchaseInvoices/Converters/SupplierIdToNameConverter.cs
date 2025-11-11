using System.Globalization;
using System.Windows.Data;
using Next_Future_ERP.Features.Suppliers.Models;
using System.Linq;
using System.Collections.ObjectModel;

namespace Next_Future_ERP.Features.PurchaseInvoices.Converters
{
    public class SupplierIdToNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return "غير محدد";

            if (values[0] is int supplierId && values[1] is ObservableCollection<Supplier> suppliers)
            {
                var supplier = suppliers.FirstOrDefault(s => s.SupplierID == supplierId);
                return supplier?.SupplierName ?? "غير محدد";
            }

            return "غير محدد";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
