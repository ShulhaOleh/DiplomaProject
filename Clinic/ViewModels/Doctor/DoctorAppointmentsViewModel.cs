using Clinic.ViewModels;

namespace Clinic.ViewModels.Doctor
{
    public class DoctorAppointmentsViewModel : BaseViewModel
    {
        private string _testMessage = "Це ViewModel";
        public string TestMessage
        {
            get => _testMessage;
            set { _testMessage = value; OnPropertyChanged(); }
        }
    }

}