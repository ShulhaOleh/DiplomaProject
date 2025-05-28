using System.Collections.ObjectModel;
using System.Windows.Input;
using Clinic.ViewModels.Admin;
using Clinic.ViewModels.Doctor;
using Clinic.ViewModels.Receptionist;
using Clinic.View;

namespace Clinic.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public string FullName { get; }
        public string Role { get; }
        public int LinkedId { get; }
        public object CurrentView { get; set; }

        public ObservableCollection<MenuItem> MenuItems { get; } = new();
        public ICommand NavigateCommand { get; }
        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public MainWindowViewModel(string fullName, string role, int linkedId)
        {
            FullName = fullName;
            Role = role;
            LinkedId = linkedId;

            NavigateCommand = new RelayCommand<BaseViewModel>(vm => CurrentViewModel = vm);

            switch (role)
            {
                case "Doctor":
                    MenuItems.Add(new MenuItem("Прийоми", new DoctorAppointmentsViewModel(LinkedId)));
                    MenuItems.Add(new MenuItem("Запис на прийом", new RegisterPatientViewModel(LinkedId)));
                    // CurrentViewModel = new DoctorAppointmentsViewModel(LinkedId);
                    break;

                case "Receptionist":
                    MenuItems.Add(new MenuItem
                    {
                        Title = "Запис на прийом",
                        ViewModel = new AppointmentRegistrationViewModel()
                    });
                    break;

                case "Admin":
                    MenuItems.Add(new MenuItem
                    {
                        Title = "Користувачі",
                        ViewModel = new AdminUserManagementViewModel()
                    });
                    break;
            }

            if (MenuItems.Count > 0 && MenuItems[0].ViewModel is BaseViewModel defaultVM)
            {
                CurrentViewModel = defaultVM;
            }
        }

        private void OpenRegisterPatientPage()
        {
            CurrentView = new RegisterPatientWindow(LinkedId);
            OnPropertyChanged(nameof(CurrentView));
        }
    }
}
