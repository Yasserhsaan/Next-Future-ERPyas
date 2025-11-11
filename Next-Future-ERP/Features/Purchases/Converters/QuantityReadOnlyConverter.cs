using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Next_Future_ERP.Features.Purchases.Converters
{
    public class QuantityReadOnlyConverter : IValueConverter
    {
        // value = الكمية الحالية، parameter = الكمية الأصلية
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0m;
            if (!decimal.TryParse(value.ToString(), out decimal qty)) return 0m;

            if (parameter != null && decimal.TryParse(parameter.ToString(), out decimal max))
            {
                if (qty > max)
                    return max; // لا يسمح بالزيادة
            }

            return qty;
        }
    }

}
