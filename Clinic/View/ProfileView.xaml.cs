using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Clinic.Properties;

namespace Clinic.View
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();

            string lang = Settings.Default.AppLanguage;
            foreach (ComboBoxItem item in LanguageSelector.Items)
            {
                if ((item.Tag as string) == lang)
                {
                    LanguageSelector.SelectedItem = item;
                    break;
                }
            }
        }

        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageSelector.SelectedItem is ComboBoxItem item)
            {
                string lang = item.Tag as string;

                Settings.Default.AppLanguage = lang;
                Settings.Default.Save();

                var ci = new CultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;

                var dict = new ResourceDictionary
                {
                    Source = new Uri($"Languages/Resources.{lang}.xaml", UriKind.Relative)
                };

                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
        }
    }
}
