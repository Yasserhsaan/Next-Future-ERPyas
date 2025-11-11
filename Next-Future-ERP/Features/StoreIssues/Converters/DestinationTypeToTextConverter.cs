using System.Globalization;
using System.Windows.Data;

namespace Next_Future_ERP.Features.StoreIssues.Converters
{
    public class DestinationTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is char destinationType)
            {
                return destinationType switch
                {
                    'E' => "مصروف",
                    'P' => "تشغيل",
                    'C' => "تكلفة مبيعات",
                    'S' => "هالك",
                    'A' => "تسوية",
                    'O' => "أخرى",
                    _ => "غير محدد"
                };
            }

            return "غير محدد";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text switch
                {
                    "مصروف" => 'E',
                    "تشغيل" => 'P',
                    "تكلفة مبيعات" => 'C',
                    "هالك" => 'S',
                    "تسوية" => 'A',
                    "أخرى" => 'O',
                    _ => 'O'
                };
            }

            return 'O';
        }
    }
}
