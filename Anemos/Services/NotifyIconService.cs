using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NotifyIconLib;
using Serilog;

namespace Anemos.Services;

public class MenuItem
{
    public TypeManaged Type;
    public string Text = string.Empty;
    public bool IsEnabled = true;
    public bool IsChecked;
    public int RadioGroup;
    public List<MenuItem> Children = new();

    public string ProfileId = string.Empty;

    public event EventHandler<EventArgs>? Click;

    public void OnClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
    }

    public void Add(MenuItem item)
    {
        Children.Add(item);
    }

    public void ClearClickEventHandler()
    {
        Click = null;
    }

    public void ClearChildren()
    {
        foreach (var child in Children)
        {
            child.ClearClickEventHandler();
        }
        Children.Clear();
    }
}

public class NotifyIconService : ObservableRecipient, INotifyIconService
{
    private readonly IFanService _fanService = App.GetService<IFanService>();

    private readonly IRuleService _ruleService = App.GetService<IRuleService>();

    private readonly FansViewModel _fansVM;

    private bool UseRules => _fanService.UseRules;

    private string CurrentProfileId => UseRules ? _fanService.CurrentAutoProfileId : _fanService.CurrentProfileId;

    private string Tooltip
    {
        get
        {
            var profile = _fanService.GetProfile(CurrentProfileId);
            if (profile == null) { return string.Empty; }

            if (UseRules)
            {
                var rule = _ruleService.CurrentRule;
                if (rule != null)
                {
                    return $"{profile.Name}\n{rule.Name}";
                }
            }
            return profile.Name;
        }
    }

    private readonly NotifyIcon _notifyIcon = NotifyIcon.Instance;

    private readonly Dictionary<int, MenuItem> _menuItemsDict = new();

    private List<MenuItem> Menu { get; } = new();

    public NotifyIconService()
    {
        Messenger.Register<AppExitMessage>(this, AppExitMessageHandler);
        Messenger.Register<RuleSwitchedMessage>(this, RuleSwitchedMessageHandler);

        _fansVM = App.GetService<FansViewModel>();

        _notifyIcon.IconClick += NotifyIcon_IconClick;
        _notifyIcon.ItemClick += NotifyIcon_ItemClick;

        SetupMenu();

        Log.Information("[NotifyIconService] Started");
    }

    private void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        Messenger.UnregisterAll(this);
        _notifyIcon.Dispose();
        Messenger.Send(new ServiceShutDownMessage(GetType().GetInterface("INotifyIconService")!));
    }

    private void RuleSwitchedMessageHandler(object recipient, RuleSwitchedMessage message)
    {
        SetupMenu();
    }

    private void NotifyIcon_IconClick()
    {
        App.ShowWindow();
    }

    private void NotifyIcon_ItemClick(int id)
    {
        if (!_menuItemsDict.TryGetValue(id, out var item)) { return; }

        if (item.Type == TypeManaged.Check)
        {
            item.IsChecked = !item.IsChecked;
            Set();
        }
        else if (item.Type == TypeManaged.Radio && !item.IsChecked)
        {
            item.IsChecked = !item.IsChecked;
            EnsureRadioCheckedOnlyMe(id);
            Set();
        }

        item.OnClick();
    }

    public void SetMenuItems(List<MenuItem> items)
    {
        ClearDict();

        foreach (var item in items)
        {
            AddMenuItem(item);
        }

        EnsureRadioCheckedNoMoreThanTwo();
        Set();
    }

    public void SetTooltip(string tooltip)
    {
        _notifyIcon.SetTooltip(tooltip);
    }

    public void SetVisible(bool visible)
    {
        _notifyIcon.SetVisible(visible);
    }

    public void SetupMenu()
    {
        ClearMenu();

        var rule = new MenuItem
        {
            Type = TypeManaged.Check,
            Text = "NotifyIcon_UseRules".GetLocalized(),
            IsChecked = UseRules
        };
        rule.Click += Rule_Click;
        Menu.Add(rule);

        var subProfile = new MenuItem
        {
            Type = TypeManaged.Submenu,
            Text = "NotifyIcon_Profiles".GetLocalized()
        };
        Menu.Add(subProfile);

        foreach (var profile in _fanService.Profiles)
        {
            var item = new MenuItem
            {
                Type = TypeManaged.Radio,
                Text = profile.Name,
                IsChecked = CurrentProfileId == profile.Id,
                ProfileId = profile.Id
            };
            item.Click += Profile_Click;
            subProfile.Add(item);
        }

        var bar = new MenuItem { Type = TypeManaged.Separator };
        Menu.Add(bar);

        var exit = new MenuItem { Text = "NotifyIcon_Exit".GetLocalized() };
        exit.Click += (sender, e) => { App.Current.RequestShutdown(); };
        Menu.Add(exit);

        SetMenuItems(Menu);

        SetTooltip(Tooltip);
    }

    public void UpdateTooltip()
    {
        OnPropertyChanged(nameof(Tooltip));
        SetTooltip(Tooltip);
    }

    private void Rule_Click(object? sender, EventArgs e)
    {
        _fansVM.UseRules = !_fansVM.UseRules;
    }

    private void Profile_Click(object? sender, EventArgs e)
    {
        if (sender is MenuItem item)
        {
            if (!_fanService.Profiles.Select(p => p.Id).Contains(item.ProfileId)) { return; }

            _fanService.CurrentProfileId = item.ProfileId;
            if (UseRules)
            {
                _fansVM.UseRules = false;
            }

            SetupMenu();
        }
    }

    private void ClearMenu()
    {
        foreach (var item in Menu)
        {
            item.ClearClickEventHandler();
        }
        Menu.Clear();
    }

    private void AddMenuItem(MenuItem item, int depth = 0)
    {
        int id;
        var itemsInDepth = _menuItemsDict.Keys.Where(k => k >= 1000 * depth && k < 1000 * (depth + 1));
        if (itemsInDepth.Any())
        {
            id = itemsInDepth.Max() + 1;
        }
        else
        {
            id = 1000 * depth;
        }

        _menuItemsDict.Add(id, item);

        foreach (var child in item.Children)
        {
            AddMenuItem(child, depth + 1);
        }
    }

    private void ClearDict()
    {
        foreach (var item in _menuItemsDict.Values)
        {
            item.ClearClickEventHandler();
        }

        _menuItemsDict.Clear();
    }

    private void EnsureRadioCheckedNoMoreThanTwo()
    {
        var groups = _menuItemsDict.Values.Where(m => m.Type == TypeManaged.Radio).Select(m => m.RadioGroup).ToHashSet();
        foreach (var group in groups)
        {
            KeyValuePair<int, MenuItem>? checkedPair = null;
            foreach (var pair in _menuItemsDict.Where(p =>
                p.Value.Type == TypeManaged.Radio &&
                p.Value.RadioGroup == group &&
                (!checkedPair.HasValue || p.Key != checkedPair.Value.Key)))
            {
                if (pair.Value.IsChecked)
                {
                    if (checkedPair != null)
                    {
                        _menuItemsDict[checkedPair.Value.Key].IsChecked = false;
                    }
                    checkedPair = pair;
                }
            }
        }
    }

    private void EnsureRadioCheckedOnlyMe(int id)
    {
        var group = _menuItemsDict[id].RadioGroup;

        foreach (var pair in _menuItemsDict.Where(p =>
            p.Value.Type == TypeManaged.Radio &&
            p.Value.RadioGroup == group &&
            p.Value.IsChecked &&
            p.Key != id))
        {
            _menuItemsDict[pair.Key].IsChecked = false;
        }
    }

    private void Set()
    {
        var menu = _menuItemsDict.Select(m =>
            new MenuItemManaged()
            {
                Id = m.Key,
                Type = m.Value.Type,
                Text = m.Value.Text,
                IsChecked = m.Value.IsChecked,
                IsEnabled = m.Value.IsEnabled
            }).ToList();
        _notifyIcon.SetMenuItems(menu);
    }
}
