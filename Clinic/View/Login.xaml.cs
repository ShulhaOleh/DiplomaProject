using Clinic.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Clinic.View
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            SetInitialLanguage();

            if (DataContext is LoginViewModel vm && !string.IsNullOrEmpty(vm.Password))
            {
                PasswordBox.Password = vm.Password;
            }
        }

        private void SetInitialLanguage()
        {
            string culture = Properties.Settings.Default.AppLanguage;

            foreach (ComboBoxItem item in LanguageSelector.Items)
            {
                if (item.Tag.ToString() == culture)
                {
                    LanguageSelector.SelectedItem = item;
                    break;
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                var password = ((PasswordBox)sender).Password;
                vm.Password = password;

                if (vm.RememberMe)
                    Properties.Settings.Default.SavedPassword = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private void ChangeLanguage(string lang)
        {
            var dict = new ResourceDictionary();

            switch (lang)
            {
                case "uk":
                    dict.Source = new Uri("Languages/Resources.uk.xaml", UriKind.Relative);
                    break;
                case "en":
                    dict.Source = new Uri("Languages/Resources.en.xaml", UriKind.Relative);
                    break;
            }

            var oldDict = Application.Current.Resources.MergedDictionaries
                           .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Resources."));
            if (oldDict != null)
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);

            Application.Current.Resources.MergedDictionaries.Add(dict);

        }

        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageSelector.SelectedItem is ComboBoxItem selected)
            {
                string lang = selected.Tag.ToString();
                ChangeLanguage(lang);

                LanguageManager.SetLanguage(lang);
            }
        }
    }
}
