using System.Collections.ObjectModel;
using System.Windows.Input;
using Clinic.ViewModels;
using Clinic.ViewModels.Admin;
using Clinic.ViewModels.Doctor;
using Clinic.ViewModels.Receptionist;

public class MainWindowViewModel : BaseViewModel
{
    public string FullName { get; }
    public string Role { get; }

    public ObservableCollection<MenuItem> MenuItems { get; }

    public ICommand NavigateCommand { get; }

    private BaseViewModel _currentViewModel;
    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        set { _currentViewModel = value; OnPropertyChanged(); }
    }

    public MainWindowViewModel(string fullName, string role)
    {
        FullName = fullName;
        Role = role;

        MenuItems = new ObservableCollection<MenuItem>();
        NavigateCommand = new RelayCommand<BaseViewModel>(vm => CurrentViewModel = vm);

        switch (role)
        {
            case "Doctor":
                MenuItems.Add(new MenuItem("Прийоми", new DoctorAppointmentsViewModel()));
                CurrentViewModel = new DoctorAppointmentsViewModel();
                break;
            case "Receptionist":
                MenuItems.Add(new MenuItem("Запис", new AppointmentRegistrationViewModel()));
                CurrentViewModel = new AppointmentRegistrationViewModel();
                break;
            case "Admin":
                MenuItems.Add(new MenuItem("Користувачі", new AdminUserManagementViewModel()));
                CurrentViewModel = new AdminUserManagementViewModel();
                break;
        }
    }
}