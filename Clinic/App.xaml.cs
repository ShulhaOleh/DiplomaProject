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
            try
            {
                base.OnStartup(e);

                string savedLang = Clinic.Properties.Settings.Default.AppLanguage;
                if (string.IsNullOrEmpty(savedLang))
                {
                    string systemLang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
                    savedLang = systemLang == "uk" ? "uk" : "en";
                    Clinic.Properties.Settings.Default.AppLanguage = savedLang;
                    Clinic.Properties.Settings.Default.Save();
                }

                Thread.CurrentThread.CurrentUICulture = new CultureInfo(savedLang);
                Thread.CurrentThread.CurrentCulture = new CultureInfo(savedLang);

                new View.Login().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FATAL ERROR: " + ex.Message);
                Environment.Exit(1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (!Clinic.Properties.Settings.Default.RememberMe)
            {
                Clinic.Properties.Settings.Default.SavedUsername = string.Empty;
                Clinic.Properties.Settings.Default.SavedPassword = string.Empty;
                Clinic.Properties.Settings.Default.RememberMe = false;
                Clinic.Properties.Settings.Default.Save();
            }
        }


    }
}