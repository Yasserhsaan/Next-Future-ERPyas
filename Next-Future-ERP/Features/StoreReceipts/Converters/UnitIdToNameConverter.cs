using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Warehouses.Models;

namespace Next_Future_ERP.Features.StoreReceipts.Converters
{
    public class UnitIdToNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int unitId && values[1] is IEnumerable<UnitModel> units)
            {
                var unit = units.FirstOrDefault(u => u.UnitID == unitId);
                return unit?.UnitName ?? "غير محدد";
            }
            return "غير محدد";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
