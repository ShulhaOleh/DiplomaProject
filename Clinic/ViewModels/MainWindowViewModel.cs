using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Clinic.ViewModels;
using Clinic.ViewModels.Doctor;


public class MainWindowViewModel : INotifyPropertyChanged
{
    public string FullName { get; }
    public string Role { get; }

    public ObservableCollection<MenuItem> MenuItems { get; }
    public ICommand NavigateCommand { get; }

    private object _currentView;
    public object CurrentView
    {
        get => _currentView;
        set { _currentView = value; OnPropertyChanged(); }
    }

    public MainWindowViewModel(string fullName, string role)
    {
        FullName = fullName;
        Role = role;

        MenuItems = GenerateMenuItems(role);
        NavigateCommand = new RelayCommand(Navigate);

        CurrentView = GetStartPage(role);
    }

    private void Navigate(object targetPage)
    {
        CurrentView = targetPage;
    }

    private ObservableCollection<MenuItem> GenerateMenuItems(string role)
    {
        var items = new ObservableCollection<MenuItem>();

        if (role == "Doctor")
            items.Add(new MenuItem("Мої прийоми", new DoctorAppointmentsViewModel()));
        else if (role == "Receptionist")
            items.Add(new MenuItem("Записати пацієнта", new AppointmentRegistrationViewModel()));
        else if (role == "Admin")
            items.Add(new MenuItem("Керування користувачами", new AdminUserManagementViewModel()));

        return items;
    }

    private object GetStartPage(string role) => role switch
    {
        "Doctor" => new DoctorAppointmentsViewModel(),
        "Receptionist" => new AppointmentRegistrationViewModel(),
        "Admin" => new AdminUserManagementViewModel(),
        _ => null
    };

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string prop = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}

