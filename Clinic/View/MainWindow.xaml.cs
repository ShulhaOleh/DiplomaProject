using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Clinic.ViewModels;
using Clinic.ViewModels.Doctor;
using Clinic.ViewModels.Receptionist;
using Clinic.ViewModels.Admin;
using Clinic.View;

namespace Clinic.View
{
    public partial class MainWindow : Window
    {
        public MainWindow(string fullName, string role, int? linkedId)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(fullName, role, linkedId ?? 0);
        }

        private void MenuItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && ((ListBox)sender).SelectedItem is Clinic.ViewModels.MenuItem item)
            {
                if (item.IsAction)
                {
                    item.Action?.Invoke();
                    ((ListBox)sender).SelectedItem = null;
                }
                if (item.ViewModel is BaseViewModel baseVM)
                {
                    vm.CurrentViewModel = baseVM;
                }
            }
        }


        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4),
                MinWidth = 160
            };

            if (DataContext is MainWindowViewModel vm)
            {
                foreach (var item in vm.MenuItems)
                {
                    if (item.ViewModel is BaseViewModel vmTarget)
                    {
                        var menuItem = new System.Windows.Controls.MenuItem
                        {
                            Header = item.Title,
                            Command = vm.NavigateCommand,
                            CommandParameter = vmTarget,
                            FontSize = 14,
                            Padding = new Thickness(12, 6, 12, 6),
                            Style = (Style)FindResource(typeof(System.Windows.Controls.MenuItem))
                        };

                        menu.Items.Add(menuItem);
                    }
                    else if (item.IsAction && item.Action != null)
                    {
                        var menuItem = new System.Windows.Controls.MenuItem
                        {
                            Header = item.Title,
                            FontSize = 14,
                            Padding = new Thickness(12, 6, 12, 6),
                            Command = new RelayCommand(item.Action),
                            Style = (Style)FindResource(typeof(System.Windows.Controls.MenuItem))
                        };

                        menu.Items.Add(menuItem);
                    }
                }

                menu.Items.Add(new Separator());

                var logoutItem = new System.Windows.Controls.MenuItem
                {
                    Header = "Розлогінитися",
                    FontSize = 14,
                    Padding = new Thickness(12, 6, 12, 6),
                    Command = new RelayCommand(Logout),
                    Style = (Style)FindResource(typeof(System.Windows.Controls.MenuItem))
                };

                var exitItem = new System.Windows.Controls.MenuItem
                {
                    Header = "Вийти",
                    FontSize = 14,
                    Padding = new Thickness(12, 6, 12, 6),
                    Command = new RelayCommand(Exit),
                    Style = (Style)FindResource(typeof(System.Windows.Controls.MenuItem))
                };

                menu.Items.Add(logoutItem);
                menu.Items.Add(exitItem);
            }

            menu.PlacementTarget = (Button)sender;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menu.IsOpen = true;
        }


        private void Exit()
        {
            Close();
        }

        private void Logout()
        {
            var loginWindow = new Login();
            loginWindow.Show();

            this.Close();
        }
    }
}
