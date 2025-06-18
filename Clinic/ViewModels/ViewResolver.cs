using Clinic.View.Admin;
using Clinic.View.Doctor;
using Clinic.View.Receptionist;
using Clinic.View;
using Clinic.ViewModels.Admin;
using Clinic.ViewModels.Doctor;
using Clinic.ViewModels.Receptionist;
using Clinic.ViewModels;
using System.Windows.Controls;
using System;

public static class ViewResolver
{
    public static UserControl ResolveView(BaseViewModel viewModel)
    {
        return viewModel switch
        {
            DoctorAppointmentsViewModel vm => new DoctorAppointmentsView { DataContext = vm },
            RegisterPatientViewModel vm => new RegisterPatientWindow(vm),
            AppointmentManagementViewModel vm => new AppointmentManagementView { DataContext = vm },
            AdminUserManagementViewModel vm => new AdminUserManagementView { DataContext = vm },
            ProfileViewModel vm => new ProfileView { DataContext = vm },
            _ => throw new NotSupportedException($"View for {viewModel.GetType().Name} is not registered.")
        };
    }
}
