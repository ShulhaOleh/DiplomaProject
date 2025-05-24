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

namespace Clinic.View
{
    public partial class MainWindow : Window
    {
        public MainWindow(string fullName, string role, int? linkedId)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(fullName, role, linkedId ?? 0);

        }


        private void Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && sender is ListBox lb && lb.SelectedItem is MenuItem item && item.ViewModel is BaseViewModel target)
            {
                vm.NavigateCommand.Execute(target);
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
                }

                menu.Items.Add(new Separator());

                var logoutItem = new System.Windows.Controls.MenuItem
                {
                    Header = "Вийти",
                    FontSize = 14,
                    Padding = new Thickness(12, 6, 12, 6),
                    Command = new RelayCommand(Logout),
                    Style = (Style)FindResource(typeof(System.Windows.Controls.MenuItem))
                };

                menu.Items.Add(logoutItem);
            }

            menu.PlacementTarget = (Button)sender;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private void Logout()
        {
            Close();
        }
    }
}