using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using NotifyIconLib.Events;
using NotifyIconLib.Native;

namespace NotifyIconLib;

public class NotifyIcon
{
    public event EventHandler<NotifyIconClickEventArgs>? Click;
    public event EventHandler<MenuItemClickEventArgs>? ItemClick;

    public Guid Guid
    {
        get; init;
    }

    public ObservableCollection<MenuItem> MenuItems { get; } = new();

    private readonly Icon _icon;

    private readonly uint _nItemsPerDepth = 10000u;

    private readonly System.Timers.Timer _timer = new(100) { AutoReset = false };

    #region CTOR

    public NotifyIcon(Guid guid)
    {
        Guid = guid;

        Bitmap bmp = new(16, 16, PixelFormat.Format1bppIndexed); // black bmp
        _icon = Icon.FromHandle(bmp.GetHicon());

        Create();
    }

    public NotifyIcon(Guid guid, Icon icon)
    {
        Guid = guid;
        _icon = icon;

        Create();
    }

    public NotifyIcon(Guid guid, string iconFile)
    {
        Guid = guid;
        _icon = new Icon(iconFile);

        Create();
    }

    #endregion CTOR

    internal void InvokeIconClick()
    {
        Click?.Invoke(this, new(this));
    }

    internal void InvokeItemClick(uint id)
    {
        var item = FlattenMenuItems().SingleOrDefault(m => m?.ItemId_ == id, null);
        if (item != null)
        {
            OnItemClick(item);
            ItemClick?.Invoke(this, new(this, item));
        }
    }

    private void OnItemClick(MenuItem item)
    {
        switch (item.Type)
        {
            case MenuItemType.Check:
                item.IsChecked = !item.IsChecked;
                NativeFunctions.SetChecked(Guid, item.ItemId_!.Value, item.IsChecked);
                break;
            case MenuItemType.Radio:
                if (item.IsChecked) { break; }
                EnsureRadioCheckedOnlyMe(item);
                break;
        }
    }

    private IEnumerable<MenuItem> FlattenMenuItems() => MenuItems.SelectMany(m => new[] { m }.Concat(m.Children));

    private void EnsureRadioCheckedOnlyMe(MenuItem item)
    {
        item.IsChecked = true;
        NativeFunctions.SetChecked(Guid, item.ItemId_!.Value, item.IsChecked);

        var others = FlattenMenuItems()
            .Where(m => m.Type == MenuItemType.Radio && m.RadioGroup == item.RadioGroup)
            .Where(m => m.IsChecked && m.ItemId_ != item.ItemId_);
        foreach (var m in others)
        {
            m.IsChecked = false;
            NativeFunctions.SetChecked(Guid, m.ItemId_!.Value, m.IsChecked);
        }
    }

    private void MenuItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        _timer.Stop();
        _timer.Start();
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        foreach (var item in MenuItems)
        {
            item.ItemId_ = null;
        }

        var flat = FlattenMenuItems();
        foreach (var item in MenuItems)
        {
            SetItemId(flat, item);
        }

        SetMenu();
    }

    private void Create()
    {
        _timer.Elapsed += Timer_Elapsed;
        MenuItems.CollectionChanged += MenuItems_CollectionChanged;

        NativeFunctions.CreateNotifyIcon(Guid, _icon);
    }

    public void Destroy()
    {
        _timer.Stop();
        MenuItems.Clear();
        NativeFunctions.DeleteNotifyIcon(Guid);
    }

    public void SetIcon(Icon icon)
    {
        NativeFunctions.SetIcon(Guid, icon);
    }

    public void SetTooltip(string toolip)
    {
        NativeFunctions.SetTooltip(Guid, toolip);
    }

    public void SetVisibility(bool visible)
    {
        NativeFunctions.SetVisibility(Guid, visible);
    }

    public void Update()
    {
        _timer.Start();
    }

    private void SetItemId(IEnumerable<MenuItem> flat, MenuItem item, uint depth = 0)
    {
        var itemsInDepth = flat.Where(m =>
            m.ItemId_ != null &&
            m.ItemId_ >= _nItemsPerDepth * depth &&
            m.ItemId_ < _nItemsPerDepth * (depth + 1u));

        uint num;
        if (itemsInDepth.Any())
        {
            num = itemsInDepth.Select(m => m.ItemId_!.Value).Max() + 1u;
        }
        else
        {
            num = _nItemsPerDepth * depth;
        }
        item.ItemId_ = num;

        foreach (var child in item.Children)
        {
            SetItemId(flat, child, depth + 1u);
        }
    }

    private void SetMenu()
    {
        if (!MenuItems.Any()) { return; }

        var menu = FlattenMenuItems().Select(m =>
        new NativeFunctions.MenuItemStruct_
        {
            Id = m.ItemId_!.Value,
            Type = m.Type,
            Text = m.Text,
            IsChecked = m.IsChecked,
            IsEnabled = m.IsEnabled,
            hIcon = m.Icon != null ? m.Icon.Handle : IntPtr.Zero,
        }).ToList();
        NativeFunctions.SetMenuItems(Guid, menu);
    }
}
