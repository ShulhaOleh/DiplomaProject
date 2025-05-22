public class MenuItem
{
    public string Title { get; set; }
    public object ViewModel { get; set; }

    public MenuItem(string title, object vm)
    {
        Title = title;
        ViewModel = vm;
    }
}