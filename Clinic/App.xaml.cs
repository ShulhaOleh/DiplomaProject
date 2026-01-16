using Clinic.DB;
using Clinic.Properties;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace Clinic
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                EnvLoader.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading .env file: {ex.Message}\n\n" +
                    "Make sure the .env file exists in the project root directory.",
                    "Configuration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var lang = Settings.Default.AppLanguage;
            if (string.IsNullOrEmpty(lang))
            {
                var sys = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
                lang = sys == "uk" ? "uk" : "en";
                Settings.Default.AppLanguage = lang;
                Settings.Default.Save();
            }
            var ci = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            var dict = new ResourceDictionary
            {
                Source = new Uri($"Languages/Resources.{lang}.xaml", UriKind.Relative)
            };
            Resources.MergedDictionaries.Add(dict);

            new View.Login().Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (!Settings.Default.RememberMe)
            {
                Settings.Default.SavedUsername = "";
                Settings.Default.SavedPassword = "";
                Settings.Default.RememberMe = false;
                Settings.Default.Save();
            }
        }

        public static int? CurrentDoctorId { get; set; }
    }
}
