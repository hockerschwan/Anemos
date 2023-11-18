using System.Drawing;
using System.Runtime.Versioning;

namespace NotifyIconLib;

[SupportedOSPlatform("windows")]
public sealed class NotifyIconLib
{
    private static readonly Lazy<NotifyIconLib> lazy = new(() => new NotifyIconLib());
    public static NotifyIconLib Instance => lazy.Value;

    private readonly Dictionary<Guid, NotifyIcon> _dict = [];

    private readonly Native.NativeFunctions.CallbackGuid _iconClickDelegate;
    private readonly Native.NativeFunctions.CallbackUint _itemClickDelegate;

    private NotifyIconLib()
    {
        _iconClickDelegate = Callback_IconClick;
        Native.NativeFunctions.SetCallback_IconClick(_iconClickDelegate);

        _itemClickDelegate = Callback_ItemClick;
        Native.NativeFunctions.SetCallback_ItemClick(_itemClickDelegate);
    }

    private void Callback_IconClick(string guid)
    {
        if (!_dict.TryGetValue(new Guid(guid), out var ni)) { return; }

        ni.InvokeIconClick();
    }

    private void Callback_ItemClick(string guid, uint id)
    {
        if (!_dict.TryGetValue(new Guid(guid), out var ni)) { return; }

        ni.InvokeItemClick(id);
    }

    public NotifyIcon CreateIcon(Guid guid, Icon? icon = null)
    {
        if (_dict.ContainsKey(guid))
        {
            throw new ArgumentException($"Guid:{guid} already exists.", nameof(guid));
        }

        NotifyIcon ni = icon == null ? new(guid) : new(guid, icon);
        _dict.Add(guid, ni);
        return ni;
    }

    public void DeleteAll()
    {
        foreach (var item in _dict)
        {
            item.Value.Destroy();
            _dict.Remove(item.Key);
        }
    }

    public void DeleteIcon(Guid guid)
    {
        if (!_dict.TryGetValue(guid, out var ni)) { return; }

        ni.Destroy();
    }

    public IEnumerable<Guid> GetGuids()
    {
        return _dict.Keys.ToList();
    }

    public NotifyIcon? GetIcon(Guid guid)
    {
        return _dict.TryGetValue(guid, out var ni) ? ni : null;
    }
}
