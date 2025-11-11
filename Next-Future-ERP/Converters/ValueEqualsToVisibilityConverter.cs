using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Next_Future_ERP.Converters
{
    public class ValueEqualsToVisibilityConverter : IValueConverter
    {
        public object? ExpectedValue { get; set; }
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var expected = parameter ?? ExpectedValue;
            bool equals = Equals(value?.ToString(), expected?.ToString());
            if (Invert) equals = !equals;
            return equals ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}


