using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Next_Future_ERP.Features.Warehouses.Models;

namespace Next_Future_ERP.Features.StoreReceipts.Converters
{
    public class WarehouseIdToNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int warehouseId && values[1] is IEnumerable<Warehouse> warehouses)
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
