using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Next_Future_ERP.Converters
{
    public class SafeDecimalConverter : IValueConverter
    {
        public string? Format { get; set; } // e.g., "F3", "N4"

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            if (value is decimal d)
                return string.IsNullOrWhiteSpace(Format) ? d.ToString(culture) : d.ToString(Format, culture);
            if (value is IFormattable formattable)
                return string.IsNullOrWhiteSpace(Format) ? formattable.ToString(null, culture) : formattable.ToString(Format, culture);
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var s = value.ToString() ?? string.Empty;

            // Normalize Arabic-Indic digits and separators, keep digits, signs and decimal separators
            var builder = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                // Arabic-Indic digits
                if (ch >= '\u0660' && ch <= '\u0669')
                {
                    builder.Append((char)('0' + (ch - '\u0660')));
                    continue;
                }
                // Eastern Arabic-Indic digits
                if (ch >= '\u06F0' && ch <= '\u06F9')
                {
                    builder.Append((char)('0' + (ch - '\u06F0')));
                    continue;
                }
                // Allowed characters
                if (char.IsDigit(ch) || ch == '-' || ch == '+' || ch == '.' || ch == ',' || ch == '\u066B' /*Arabic decimal separator*/ || ch == '\u066C' /*Arabic thousands separator*/)
                {
                    builder.Append(ch);
                }
                // ignore all others (like currency/letters)
            }

            var normalized = builder.ToString();

            // Replace Arabic decimal separator (\u066B) with dot
            normalized = normalized.Replace('\u066B', '.');
            // Remove thousands separators: comma and Arabic thousands (\u066C)
            normalized = normalized.Replace("\u066C", string.Empty).Replace(",", string.Empty);

            if (string.IsNullOrWhiteSpace(normalized)) return null;

            if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            // Fallback: current culture
            if (decimal.TryParse(normalized, NumberStyles.Any, culture, out result))
                return result;

            // إذا لم ينجح التحويل: أعد قيمة افتراضية مناسبة حسب نوع الهدف
            bool targetIsNullable = Nullable.GetUnderlyingType(targetType) != null;
            if (targetIsNullable)
                return null;

            return 0m;
        }
    }
}


