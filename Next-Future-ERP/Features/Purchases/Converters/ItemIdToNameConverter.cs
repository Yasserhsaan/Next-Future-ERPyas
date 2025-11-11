using Next_Future_ERP.Features.Items.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Next_Future_ERP.Features.Purchases.Converters
{
    public class ItemIdToNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return string.Empty;

            var itemId = (int)values[0];
            var items = values[1] as IEnumerable<Item>;
            if (items == null) return string.Empty;

            return items.FirstOrDefault(x => x.ItemID == itemId)?.ItemName ?? string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
