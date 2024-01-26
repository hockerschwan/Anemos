using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NotifyIconLib.Events;
using NotifyIconLib.Native;

namespace NotifyIconLib;

public partial class NotifyIcon
{
    public event EventHandler<NotifyIconClickEventArgs>? Click;
    public event EventHandler<MenuItemClickEventArgs>? ItemClick;

    private Guid _guid;
    public Guid Guid
    {
        get => _guid;
        init
        {
            _guid = value;
            GuidString = value.ToString().ToLower();
        }
    }

    private string GuidString { get; init; } = string.Empty;

    public List<MenuItem> MenuItems { get; } = [];

    private Icon _icon;

    private static readonly uint _nItemsPerDepth = 10000u;

    private readonly System.Timers.Timer _timer = new(100) { AutoReset = false };

    private bool _closing;

    #region CTOR

    public NotifyIcon(Guid guid)
    {
        Guid = guid;

        Bitmap bmp = new(16, 16, PixelFormat.Format1bppIndexed); // black bmp
        _icon = Icon.FromHandle(bmp.GetHicon());

        Create();

        DestroyIcon(_icon.Handle);
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
        var item = FirstOrDefault(this, id);
        if (item != null)
        {
            OnItemClick(ref item);
            ItemClick?.Invoke(this, new(this, item));
        }

        static MenuItem? FirstOrDefault(NotifyIcon @this, uint id)
        {
            foreach (var m in @this.FlattenMenuItems().ToList())
            {
                if (m.ItemId_ == id) { return m; }
            }
            return null;
        }
    }

    private void OnItemClick(ref MenuItem item)
    {
        switch (item.Type)
        {
            case MenuItemType.Check:
                item.IsChecked = !item.IsChecked;
                NativeFunctions.SetChecked(GuidString, item.ItemId_!.Value, item.IsChecked);
                break;
            case MenuItemType.Radio:
                if (item.IsChecked) { break; }
                EnsureRadioCheckedOnlyMe(ref item);
                break;
        }
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        foreach (var item in MenuItems)
        {
            item.ItemId_ = null;
        }

        var flat = FlattenMenuItems().ToList();
        foreach (var item in MenuItems)
        {
            SetItemId(flat, item);
        }

        SetMenu();
    }

    private void Create()
    {
        _timer.Elapsed += Timer_Elapsed;

        NativeFunctions.CreateNotifyIcon(GuidString, _icon);
    }

    public void Destroy()
    {
        _closing = true;
        _timer.Stop();
        MenuItems.Clear();
        NativeFunctions.DeleteNotifyIcon(GuidString);
    }

    private void EnsureRadioCheckedOnlyMe(ref MenuItem item)
    {
        item.IsChecked = true;
        NativeFunctions.SetChecked(GuidString, item.ItemId_!.Value, item.IsChecked);

        var parent = FindParent(item);
        if (parent == null) { return; }

        foreach (var child in parent.Children)
        {
            if (child.IsChecked && child.ItemId_ != null && child != item)
            {
                child.IsChecked = false;
                NativeFunctions.SetChecked(GuidString, child.ItemId_.Value, false);
            }
        }
    }

    private MenuItem? FindParent(in MenuItem item)
    {
        if (item.ItemId_ == null) { return null; }

        var d = item.ItemId_ / _nItemsPerDepth;
        var idHigh = d * _nItemsPerDepth;
        var idLow = idHigh - _nItemsPerDepth;

        foreach (var m in FlattenMenuItems().ToList())
        {
            if (m.ItemId_ >= idLow && m.ItemId_ < idHigh && m.Children.Contains(item)) { return m; }
        }
        return null;
    }

    private IEnumerable<MenuItem> FlattenMenuItems() => Traverse(MenuItems);

    private IEnumerable<MenuItem> GetItemsInDepth(List<MenuItem> flatten, uint depth)
    {
        foreach (var m in flatten)
        {
            if (m.ItemId_ != null && m.ItemId_ / _nItemsPerDepth == depth)
            {
                yield return m;
            }
        }
    }

    public void SetIcon(in Icon icon)
    {
        if (_closing) { return; }

        _icon = icon;
        NativeFunctions.SetIcon(GuidString, _icon);
    }

    private void SetItemId(in List<MenuItem> flat, MenuItem item, uint depth = 0)
    {
        var itemsInDepth = GetItemsInDepth(flat, depth).ToList();

        uint num;
        if (itemsInDepth.Count > 0)
        {
            num = GetNewId(itemsInDepth);

            static uint GetNewId(in List<MenuItem> itemsInDepth)
            {
                uint max = 0;
                foreach (var m in itemsInDepth)
                {
                    if (m.ItemId_ > max) { max = m.ItemId_.Value; }
                }
                return ++max;
            }
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
        if (MenuItems.Count == 0) { return; }

        var menu = FlattenMenuItems()
            .Select(m =>
                new NativeFunctions.MenuItemStruct_
                {
                    Id = m.ItemId_!.Value,
                    Type = m.Type,
                    Text = m.Text,
                    IsChecked = m.IsChecked,
                    IsEnabled = m.IsEnabled,
                    hIcon = m.Icon != null ? m.Icon.Handle : IntPtr.Zero,
                })
            .ToList();
        NativeFunctions.SetMenuItems(GuidString, menu);
    }

    public void SetTooltip(string tooltip)
    {
        if (_closing) { return; }

        NativeFunctions.SetTooltip(GuidString, tooltip);
    }

    public void SetVisibility(bool visible)
    {
        if (_closing) { return; }

        NativeFunctions.SetVisibility(GuidString, visible);
    }

    private IEnumerable<MenuItem> Traverse(IEnumerable<MenuItem> list)
    {
        var stack = new Stack<MenuItem>(list.Reverse());
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;
            for (var i = current.Children.Count - 1; i >= 0; --i)
            {
                stack.Push(current.Children[i]);
            }
        }
    }
    public void Update()
    {
        _timer.Start();
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyIcon(IntPtr handle);
}
