public class MenuItem
{
    public string Title { get; set; }
    public object TargetViewModel { get; set; }

    public MenuItem(string title, object target)
    {
        Title = title;
        TargetViewModel = target;
    }
}
