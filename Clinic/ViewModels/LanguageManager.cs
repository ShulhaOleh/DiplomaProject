using System;
using System.Globalization;
using System.Threading;
using System.Windows;
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

        /// <summary>
        /// Returns localized specialty name by SpecialtyID using resource keys.
        /// Falls back to the provided DB name if no resource key is found.
        /// </summary>
        public static string GetSpecialtyName(int specialtyId, string fallbackName)
        {
            var key = $"Specialty_{specialtyId}";
            try
            {
                if (Application.Current.FindResource(key) is string localized)
                    return localized;
            }
            catch (ResourceReferenceKeyNotFoundException) { }
            return fallbackName;
        }
    }
}
