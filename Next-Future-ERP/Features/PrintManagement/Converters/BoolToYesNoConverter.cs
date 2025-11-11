using System;
using System.Globalization;
using System.Windows.Data;

namespace Next_Future_ERP.Features.PrintManagement.Converters
{
    /// <summary>
    /// محول لتحويل القيم المنطقية إلى نعم/لا
    /// </summary>
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "نعم" : "لا";
            }
            
            return "لا";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue == "نعم";
            }
            
            return false;
        }
    }
}
