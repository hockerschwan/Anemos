namespace NotifyIconLib.Events;

public class NotifyIconClickEventArgs(NotifyIcon icon) : EventArgs
{
    public NotifyIcon SourceIcon { get; init; } = icon;
}

public class MenuItemClickEventArgs(NotifyIcon icon, MenuItem item) : EventArgs
{
    public NotifyIcon SourceIcon { get; init; } = icon;

    public MenuItem SourceItem { get; init; } = item;
}
