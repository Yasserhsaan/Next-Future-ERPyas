using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Next_Future_ERP.Features.Purchases.Converters
{
    public sealed class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            try
            {
                var num = System.Convert.ToInt32(value);
                return num switch
                {
                    0 => "مسودة",
                    1 => "مرحل",
                    2 => "معتمد",
                    9 => "ملغي",
                    _ => $"غير معروف ({num})"
                };
            }
            catch { return ""; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
