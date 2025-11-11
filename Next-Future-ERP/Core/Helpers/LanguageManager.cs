using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Core.Helpers
{
    class LanguageManager
    {
        public static void ChangeLanguage(string langCode)
        {
            var culture = new CultureInfo(langCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
