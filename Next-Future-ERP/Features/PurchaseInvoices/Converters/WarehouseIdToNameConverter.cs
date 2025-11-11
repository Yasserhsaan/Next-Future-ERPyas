using System.Globalization;
using System.Windows.Data;
using Next_Future_ERP.Features.Warehouses.Models;
using System.Linq;
using System.Collections.ObjectModel;

namespace Next_Future_ERP.Features.PurchaseInvoices.Converters
{
    public class WarehouseIdToNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return "غير محدد";

            if (values[0] is int warehouseId && values[1] is ObservableCollection<Warehouse> warehouses)
            {
                var warehouse = warehouses.FirstOrDefault(w => w.WarehouseID == warehouseId);
                return warehouse?.WarehouseName ?? "غير محدد";
            }

            return "غير محدد";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}