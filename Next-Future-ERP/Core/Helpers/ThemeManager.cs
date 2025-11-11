using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Core.Helpers
{
    class ThemeManager
    {
        public static void ApplyTheme(string themeName)
        {
            var uri = new Uri($"/Resources/Themes/{themeName}Theme.xaml", UriKind.Relative);
            var newDict = new ResourceDictionary { Source = uri };

            var oldDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme"));

            if (oldDict != null)
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);

            Application.Current.Resources.MergedDictionaries.Add(newDict);
        }
    }
}
