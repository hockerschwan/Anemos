namespace NotifyIconLib.Events;

public class NotifyIconClickEventArgs : EventArgs
{
    public NotifyIcon SourceIcon
    {
        get; init;
    }

    public NotifyIconClickEventArgs(NotifyIcon icon)
    {
        SourceIcon = icon;
    }
}

public class MenuItemClickEventArgs : EventArgs
{
    public NotifyIcon SourceIcon
    {
        get; init;
    }

    public MenuItem SourceItem
    {
        get; init;
    }

    public MenuItemClickEventArgs(NotifyIcon icon, MenuItem item)
    {
        SourceIcon = icon;
        SourceItem = item;
    }
}
