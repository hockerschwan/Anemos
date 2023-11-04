using System.Diagnostics;
using System.Text;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Views;
using CommunityToolkit.Mvvm.Messaging;
using NotifyIconLib;
using Serilog;

namespace Anemos.Services;

internal enum ItemRoles
{
    Default, Exit, Rule, Profile
}

[DebuggerDisplay("{Role}, {ProfileId}")]
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
    private readonly ISettingsService _settingsService;
    private readonly IFanService _fanService;
    private readonly IRuleService _ruleService;

    private readonly NotifyIconLib.NotifyIconLib _notifyIconLib = NotifyIconLib.NotifyIconLib.Instance;

    private readonly Guid _guid;
    private readonly NotifyIcon _notifyIcon;
    private string _tooltip = string.Empty;

    public NotifyIconService(
        IMessenger messenger,
        ISettingsService settingsService,
        IFanService fanService,
        IRuleService ruleService)
    {
        _messenger = messenger;
        _settingsService = settingsService;
        _fanService = fanService;
        _ruleService = ruleService;

        _messenger.Register<AppExitMessage>(this, AppExitMessageHandler);
        _messenger.Register<FanProfilesChangedMessage>(this, FanProfilesChangedMessageHandler);
        _messenger.Register<FanProfileRenamedMessage>(this, FanProfileRenamedMessageHandler);
        _messenger.Register<FanProfileSwitchedMessage>(this, FanProfileSwitchedMessageHandler);

        _settingsService.Settings.FanSettings.PropertyChanged += FanSettings_PropertyChanged;


        _guid = GenerateGuid(App.AppLocation);
        Log.Debug("[NotifyIcon] GUID: {guid}", _guid.ToString());

        var icon = System.Drawing.Icon.ExtractAssociatedIcon(App.AppLocation);
        _notifyIcon = _notifyIconLib.CreateIcon(_guid, icon);

        _notifyIcon.Click += NotifyIcon_Click;
        _notifyIcon.ItemClick += NotifyIcon_ItemClick;

        CreateCommonItems();

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Debug("[NotifyIcon] Started");
        _settingsService = settingsService;
    }

    private void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _messenger.UnregisterAll(this);
        _notifyIconLib.DeleteAll();
        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
    }

    private async void FanProfilesChangedMessageHandler(object recipient, FanProfilesChangedMessage message)
    {
        while (FansPage.RenameDialogOpened)
        {
            await Task.Delay(100);
        }
        UpdateMenu();
    }

    private async void FanProfileRenamedMessageHandler(object recipient, FanProfileRenamedMessage message)
    {
        await Task.Delay(500);
        UpdateMenu();
    }

    private void FanProfileSwitchedMessageHandler(object recipient, FanProfileSwitchedMessage message)
    {
        UpdateMenu();
    }

    private void FanSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.FanSettings.UseRules))
        {
            _notifyIcon.MenuItems[0].IsChecked = _settingsService.Settings.FanSettings.UseRules;
            UpdateMenu();
        }
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
            case ItemRoles.Rule:
                _fanService.UseRules = !_fanService.UseRules;
                break;
            case ItemRoles.Profile:
                if (item.ProfileId != null)
                {
                    _fanService.ManualProfileId = item.ProfileId;
                }
                break;
        }
    }

    private void CreateCommonItems()
    {
        var useRules = new MyMenuItem()
        {
            Type = MenuItemType.Check,
            Text = "NotifyIcon_UseRules".GetLocalized(),
            IsChecked = _fanService.UseRules,
            Role = ItemRoles.Rule,
        };

        var sub = new MyMenuItem()
        {
            Type = MenuItemType.Submenu,
            Text = "NotifyIcon_Profiles".GetLocalized(),
        };

        var bar = new MyMenuItem()
        {
            Type = MenuItemType.Separator,
        };

        var iconX = Helpers.RuntimeHelper.ExtractIcon("shell32.dll", -240);
        var exit = new MyMenuItem()
        {
            Text = "NotifyIcon_Exit".GetLocalized(),
            Icon = iconX,
            Role = ItemRoles.Exit,
        };

        _notifyIcon.MenuItems.Add(useRules);
        _notifyIcon.MenuItems.Add(sub);
        _notifyIcon.MenuItems.Add(bar);
        _notifyIcon.MenuItems.Add(exit);
    }

    private static Guid GenerateGuid(string str)
    {
        var hash = System.Security.Cryptography.SHA1.HashData(Encoding.UTF8.GetBytes(str))[..16];
        hash[6] = (byte)((hash[6] & 0x0f) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3f) | 0x80);

        var s = new StringBuilder();
        foreach (var b in hash)
        {
            s.Append(b.ToString("x2"));
        }

        return new Guid(s.ToString());
    }

    public void SetTooltip(string tooltip) => _notifyIcon.SetTooltip(tooltip);

    public void SetVisibility(bool visible) => _notifyIcon.SetVisibility(visible);

    public void Update() => UpdateMenu();

    private void UpdateMenu()
    {
        var pr = _fanService.GetProfile(_fanService.UseRules ? _fanService.AutoProfileId : _fanService.ManualProfileId);
        var sub = _notifyIcon.MenuItems[1];
        if (sub.Children.Cast<MyMenuItem>()
            .Select(x => x.ProfileId)
            .SequenceEqual(_fanService.Profiles.Select(x => x.Id)))
        {
            foreach (var item in sub.Children.Cast<MyMenuItem>())
            {
                item.Text = _fanService.GetProfile(item.ProfileId!)!.Name;
                item.IsChecked = pr?.Id == item.ProfileId;
            }
        }
        else
        {
            sub.Children.Clear();
            foreach (var profile in _fanService.Profiles)
            {
                var item = new MyMenuItem()
                {
                    Type = MenuItemType.Radio,
                    Text = profile.Name,
                    IsChecked = pr?.Id == profile.Id,
                    RadioGroup = 1,
                    Role = ItemRoles.Profile,
                    ProfileId = profile.Id,
                };
                sub.Children.Add(item);
            }
        }

        _notifyIcon.MenuItems[0].IsChecked = _fanService.UseRules;

        UpdateTooltip();

        _notifyIcon.Update();
    }

    private void UpdateTooltip()
    {
        if (_fanService.UseRules)
        {
            var profile = _fanService.GetProfile(_fanService.AutoProfileId);
            var rule = _ruleService.CurrentRule;
            _tooltip = $"{profile?.Name}\n{rule?.Name}";
        }
        else
        {
            var profile = _fanService.GetProfile(_fanService.ManualProfileId);
            _tooltip = profile?.Name ?? string.Empty;
        }

        _notifyIcon.SetTooltip(_tooltip);
    }
}
