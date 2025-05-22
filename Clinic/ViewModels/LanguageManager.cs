using System;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;

namespace Clinic.ViewModels
{
    public static class LanguageManager
    {
        public static void SetLanguage(string culture)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Clinic.Properties.Settings.Default.AppLanguage = culture;
            Clinic.Properties.Settings.Default.Save();
        }

        public static string GetCurrentLanguage()
        {
            return Properties.Settings.Default.AppLanguage;
        }


    }

}