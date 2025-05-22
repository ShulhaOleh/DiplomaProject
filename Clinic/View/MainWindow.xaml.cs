using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Clinic.ViewModels;

namespace Clinic.View
{
    public partial class MainWindow : Window
    {
        public MainWindow(string fullName, string role)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(fullName, role);
        }
        private void Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && sender is ListBox lb && lb.SelectedItem is MenuItem item)
            {
                vm.NavigateCommand.Execute(item.TargetViewModel);
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
                    var menuItem = new System.Windows.Controls.MenuItem
                    {
                        Header = item.Title,
                        Command = vm.NavigateCommand,
                        CommandParameter = item.TargetViewModel,
                        FontSize = 14,
                        Padding = new Thickness(12, 6, 12, 6),
                        Icon = new TextBlock
                        {
                            Text = "",
                            FontSize = 12,
                            Margin = new Thickness(0, 0, 6, 0)
                        }
                    };

                    menuItem.Style = (Style)FindResource(typeof(System.Windows.Controls.MenuItem));

                    menu.Items.Add(menuItem);
                }

                menu.Items.Add(new Separator());

                var logoutItem = new System.Windows.Controls.MenuItem
                {
                    Header = "Вийти",
                    FontSize = 14,
                    Padding = new Thickness(12, 6, 12, 6),
                    Icon = new TextBlock
                    {
                        Text = "",
                        FontSize = 12,
                        Margin = new Thickness(0, 0, 6, 0)
                    },
                    Command = new RelayCommand(Logout)
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
