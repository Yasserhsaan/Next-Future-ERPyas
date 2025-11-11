using Next_Future_ERP.Features.Suppliers.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Next_Future_ERP.Features.Purchases.Converters
{
    public sealed class SupplierIdToNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values?.Length >= 2 &&
                    values[0] is int id &&
                    values[1] is IEnumerable<Supplier> suppliers &&
                    id > 0)
                {
                    var name = suppliers.FirstOrDefault(s => s.SupplierID == id)?.SupplierName;
                    return string.IsNullOrWhiteSpace(name) ? $"#{id}" : name;
                }
            }
            catch { /* تجاهل */ }
            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}