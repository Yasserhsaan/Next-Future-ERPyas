using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Next_Future_ERP.Features.PosStations.Converters
{
    /// <summary>
    /// محول حالة النشاط إلى نص
    /// </summary>
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "نشط" : "غير نشط";
            }
            return "غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status == "نشط";
            }
            return false;
        }
    }

    /// <summary>
    /// محول عكسي للقيم المنطقية
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    /// <summary>
    /// محول معرف المستخدم إلى اسم المستخدم
    /// </summary>
    public class UserIdToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int userId && userId > 0)
            {
                // يمكن تحسين هذا لاحقاً لاستخدام خدمة المستخدمين
                return $"مستخدم {userId}";
            }
            return "غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}