using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Next_Future_ERP.Features.Accounts.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte status)
            {
                return status switch
                {
                    0 => Colors.Orange,    // Draft - برتقالي
                    1 => Colors.Green,     // Posted - أخضر
                    _ => Colors.Gray       // Unknown - رمادي
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BalanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal balance)
            {
                if (Math.Abs(balance) < 0.01m)
                    return Colors.Green;        // متوازن - أخضر
                else if (balance > 0)
                    return Colors.DarkBlue;     // مدين - أزرق داكن
                else
                    return Colors.DarkRed;      // دائن - أحمر داكن
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Colors.Green : Colors.Red;
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BalanceStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBalanced)
            {
                return isBalanced ? "✅ متوازن" : "⚠️ غير متوازن";
            }
            return "❓ غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
