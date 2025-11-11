using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Next_Future_ERP.Converters
{
    public class BooleanToBackgroundConverter : IValueConverter
    {
        public Brush TrueBrush { get; set; } = Brushes.LightGreen;
        public Brush FalseBrush { get; set; } = Brushes.LightCoral;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? TrueBrush : FalseBrush;

            return FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
