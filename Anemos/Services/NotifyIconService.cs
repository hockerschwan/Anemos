using System.Text;
using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.Messaging;
using NotifyIconLib;
using Serilog;

namespace Anemos.Services;

internal enum ItemRoles
{
    Default, Exit, Rule, Profile
}

internal class MyMenuItem : MenuItem
{
    public ItemRoles Role
    {
        get; init;
    }

    public string? ProfileId
    {
        get; init;
    }
}

public class NotifyIconService : INotifyIconService
{
    private readonly IMessenger _messenger;

    private readonly NotifyIconLib.NotifyIconLib _notifyIconLib = NotifyIconLib.NotifyIconLib.Instance;

    private readonly Guid _guid;
    private readonly NotifyIcon _notifyIcon;

    public NotifyIconService(IMessenger messenger)
    {
        _messenger = messenger;

        _messenger.Register<AppExitMessage>(this, AppExitMessageHandler);

        _notifyIconLib.Close += NotifyIconLib_Close;

        _guid = GenerateGuid(App.AppLocation);

        var icon = System.Drawing.Icon.ExtractAssociatedIcon(App.AppLocation);
        _notifyIcon = _notifyIconLib.CreateIcon(_guid, icon);

        _notifyIcon.Click += NotifyIcon_Click;
        _notifyIcon.ItemClick += NotifyIcon_ItemClick;

        CreateCommonItems();

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Debug("[NotifyIcon] Started");
    }

    private void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _messenger.UnregisterAll(this);
        _notifyIconLib.DeleteAll();
        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
    }

    private void NotifyIconLib_Close(object? sender, EventArgs e)
    {
        App.Current.Shutdown(true);
    }

    private void NotifyIcon_Click(object? sender, NotifyIconLib.Events.NotifyIconClickEventArgs e)
    {
        App.ShowWindow();
    }

    private void NotifyIcon_ItemClick(object? sender, NotifyIconLib.Events.MenuItemClickEventArgs e)
    {
        if (e.SourceItem is not MyMenuItem item) { return; }

        switch (item.Role)
        {
            case ItemRoles.Exit:
                App.ShowWindow();
                App.Current.Shutdown();
                break;
        }
    }

    private void CreateCommonItems()
    {
        var iconX = Helpers.RuntimeHelper.ExtractIcon("shell32.dll", -240);
        var exit = new MyMenuItem()
        {
            Text = "Exit",
            Icon = iconX,
            Role = ItemRoles.Exit,
        };

        _notifyIcon.MenuItems.Add(exit);
    }

    private static Guid GenerateGuid(string str)
    {
        var hash = System.Security.Cryptography.SHA1.HashData(Encoding.UTF8.GetBytes(str));
        return new Guid(hash[0..16]);
    }

    public void SetTooltip(string tooltip) => _notifyIcon.SetTooltip(tooltip);

    public void SetVisibility(bool visible) => _notifyIcon.SetVisibility(visible);
}
