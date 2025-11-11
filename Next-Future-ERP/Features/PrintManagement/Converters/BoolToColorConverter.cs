using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Next_Future_ERP.Features.PrintManagement.Converters
{
    /// <summary>
    /// محول لتحويل القيم المنطقية إلى ألوان
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue 
                    ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) // أخضر للإيجابي
                    : new SolidColorBrush(Color.FromRgb(107, 114, 128)); // رمادي للسلبي
            }
            
            return new SolidColorBrush(Color.FromRgb(107, 114, 128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
