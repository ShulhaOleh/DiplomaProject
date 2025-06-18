using System.Collections.ObjectModel;
using System.Windows.Input;
using Clinic.ViewModels.Admin;
using Clinic.ViewModels.Doctor;
using Clinic.ViewModels.Receptionist;
using Clinic.View;
using System.Windows;

namespace Clinic.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public string FullName { get; }
        public string Role { get; }
        public int LinkedId { get; }
        public string Username { get; }

        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public object CurrentView => ViewResolver.ResolveView(CurrentViewModel);

        public ObservableCollection<MenuItem> MenuItems { get; } = new();
        public ICommand NavigateCommand { get; }


        public MainWindowViewModel(string fullName, string role, int linkedId, string username)
        {
            FullName = fullName;
            Role = role;
            LinkedId = linkedId;

            NavigateCommand = new RelayCommand<BaseViewModel>(vm => CurrentViewModel = vm);

            if (Role == "Doctor")
            {
                MenuItems.Add(new MenuItem("Прийоми", new DoctorAppointmentsViewModel(LinkedId)));
                MenuItems.Add(new MenuItem("Запис на прийом", new RegisterPatientViewModel(LinkedId, "Doctor")));
                MenuItems.Add(new MenuItem("Профіль", new ProfileViewModel(LinkedId, "Doctor")));
            }
            else if (Role == "Receptionist")
            {
                MenuItems.Add(new MenuItem("Керування прийомами", new AppointmentManagementViewModel(LinkedId)));
                MenuItems.Add(new MenuItem("Запис на прийом", new RegisterPatientViewModel(LinkedId, "Receptionist")));
                MenuItems.Add(new MenuItem("Профіль", new ProfileViewModel(LinkedId, "Receptionist")));
            }
            else if (Role == "Admin")
            {
                MenuItems.Add(new MenuItem("Керування користувачами", new AdminUserManagementViewModel(username)));
            }

            if (MenuItems.Count > 0)
                CurrentViewModel = MenuItems[0].ViewModel;
        }

    }
}
