using Next_Future_ERP.Features.Warehouses.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Next_Future_ERP.Features.Purchases.Converters
{
    public sealed class UnitIdToNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 2 && values[0] is int id && values[1] is IEnumerable<UnitModel> units)
                return units.FirstOrDefault(u => u.UnitID == id)?.UnitName ?? "";
            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
