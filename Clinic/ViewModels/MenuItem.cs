using System;

namespace Clinic.ViewModels
{
    public class MenuItem
    {
        public string Title { get; set; }
        public BaseViewModel ViewModel { get; set; }
        public bool IsAction { get; set; }
        public Action Action { get; set; }

        public MenuItem() { }

        public MenuItem(string title, BaseViewModel viewModel)
        {
            Title = title;
            ViewModel = viewModel;
        }

        public MenuItem(string title, Action action)
        {
            Title = title;
            IsAction = true;
            Action = action;
        }
    }
}
