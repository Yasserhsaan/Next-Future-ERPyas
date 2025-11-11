using System.Globalization;
using System.Windows.Data;

namespace Next_Future_ERP.Features.StoreIssues.Converters
{
    public class PoliciesToTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 3 && 
                values[0] is bool usesCostCenter && 
                values[1] is bool allowAccountOverride && 
                values[2] is bool allowLineOverride)
            {
                var policies = new List<string>();
                
                if (usesCostCenter) policies.Add("مركز كلفة");
                if (allowAccountOverride) policies.Add("تجاوز حساب");
                if (allowLineOverride) policies.Add("تجاوز سطر");
                
                return policies.Count > 0 ? string.Join(", ", policies) : "لا توجد";
            }

            return "لا توجد";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
