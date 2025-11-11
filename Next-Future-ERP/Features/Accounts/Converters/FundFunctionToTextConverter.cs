// Converters/FundFunctionToTextConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using Next_Future_ERP.Features.Accounts.Models;

namespace Next_Future_ERP.Models
{
    public class FundFunctionToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not FundType func) return "";

            // اختياري: إجبار لغة معينة عبر ConverterParameter
            var forced = parameter as string;
            var isArabic = !string.IsNullOrWhiteSpace(forced)
                ? forced.Equals("ar", StringComparison.OrdinalIgnoreCase)
                : CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase);

            if (isArabic)
            {
                return func switch
                {
                    FundType.ReceiptOnly => "للْقبض فقط",
                    FundType.PaymentOnly => "للصرف فقط",
                    FundType.Both => "للْقبض والصرف",
                    _ => ""
                };
            }

            // English
            return func switch
            {
                FundType.ReceiptOnly => "Receipt only",
                FundType.PaymentOnly => "Payment only",
                FundType.Both => "Both (receipt & payment)",
                _ => ""
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing; // لن نستخدمه مع ItemTemplate
    }
}
