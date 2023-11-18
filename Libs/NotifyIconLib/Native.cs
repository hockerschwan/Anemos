using System.Runtime.InteropServices;

namespace NotifyIconLib.Native;

internal partial class NativeFunctions
{
    #region Internal

    [StructLayout(LayoutKind.Sequential)]
    internal struct MenuItemStruct_(
        uint id,
        MenuItemType type = 0,
        string text = "",
        bool isChecked = false,
        bool isEnabled = true)
    {
        public uint Id = id;
        public MenuItemType Type = type;
        [MarshalAs(UnmanagedType.LPWStr)] public string Text = text;
        [MarshalAs(UnmanagedType.Bool)] public bool IsChecked = isChecked;
        [MarshalAs(UnmanagedType.Bool)] public bool IsEnabled = isEnabled;
        public IntPtr hIcon;
    }

    internal delegate void CallbackVoid();
    internal delegate void CallbackGuid([MarshalAs(UnmanagedType.LPStr)] string guid);
    internal delegate void CallbackUint([MarshalAs(UnmanagedType.LPStr)] string guid, uint value);

    internal static void CreateNotifyIcon(string guid, in System.Drawing.Icon icon)
    {
        var hicon = icon.Handle;
        CreateNotifyIcon_(guid.ToString().ToLower(), hicon);
    }

    internal static void DeleteNotifyIcon(string guid)
    {
        DeleteNotifyIcon_(guid);
    }

    internal static void SetCallback_IconClick(CallbackGuid callback)
    {
        SetCallback_IconClick_(Marshal.GetFunctionPointerForDelegate(callback));
    }

    internal static void SetCallback_ItemClick(CallbackUint callback)
    {
        SetCallback_ItemClick_(Marshal.GetFunctionPointerForDelegate(callback));
    }

    internal static void SetIcon(string guid, in System.Drawing.Icon icon)
    {
        SetIcon_(guid, icon.Handle);
    }

    internal static void SetMenuItems(string guid, in IList<MenuItemStruct_> menuItems)
    {
        var ptrArr = new IntPtr[menuItems.Count];
        for (var i = 0; i < menuItems.Count; ++i)
        {
            ptrArr[i] = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(MenuItemStruct_)));
            Marshal.StructureToPtr(menuItems[i], ptrArr[i], false);
        }
        SetMenuItems_(guid, ptrArr.Length, ptrArr);
    }

    internal static void SetTooltip(string guid, string tooltip)
    {
        SetTooltip_(guid, tooltip);
    }

    internal static void SetVisibility(string guid, bool visible)
    {
        SetVisibility_(guid, visible);
    }

    internal static void SetChecked(string guid, uint itemId, bool checked_)
    {
        SetChecked_(guid, (nint)itemId, checked_);
    }

    internal static void SetEnabled(string guid, uint itemId, bool enabled)
    {
        SetEnabled_(guid, (nint)itemId, enabled);
    }

    #endregion

    #region Native functions

    [LibraryImport("NotifyIconLibCpp.dll", EntryPoint = "CreateNotifyIcon")]
    private static partial void CreateNotifyIcon_(
        [MarshalAs(UnmanagedType.LPStr)] string guid,
        nint hicon);

    [LibraryImport("NotifyIconLibCpp.dll", EntryPoint = "DeleteNotifyIcon")]
    private static partial void DeleteNotifyIcon_([MarshalAs(UnmanagedType.LPStr)] string guid);

    [LibraryImport("NotifyIconLibCpp.dll", EntryPoint = "SetCallback_IconClick")]
    private static partial void SetCallback_IconClick_(nint callback);

    [LibraryImport("NotifyIconLibCpp.dll", EntryPoint = "SetCallback_ItemClick")]
    private static partial void SetCallback_ItemClick_(nint callback);

    [LibraryImport("NotifyIconLibCpp.dll", EntryPoint = "SetIcon")]
    private static partial void SetIcon_(
        [MarshalAs(UnmanagedType.LPStr)] string guid,
        nint hicon);

    [LibraryImport("NotifyIconLibCpp.dll", EntryPoint = "SetMenuItems")]
    private static partial void SetMenuItems_(
        [MarshalAs(UnmanagedType.LPStr)] string guid,
        int count, [In] IntPtr[] items);

    [LibraryImport("NotifyIconLibCpp.dll", EntryPoint = "SetTooltip")]
    private static partial void SetTooltip_(
        [MarshalAs(UnmanagedType.LPStr)] string guid,
        [MarshalAs(UnmanagedType.LPWStr)] string tooltip);

    [LibraryImport("NotifyIconLibCpp.dll", EntryPoint = "SetVisibility")]
    private static partial void SetVisibility_(
        [MarshalAs(UnmanagedType.LPStr)] string guid,
        [MarshalAs(UnmanagedType.Bool)] bool visible);

    [LibraryImport("NotifyIconLibCpp.dll", EntryPoint = "SetChecked")]
    private static partial void SetChecked_(
        [MarshalAs(UnmanagedType.LPStr)] string guid,
        nint itemId,
        [MarshalAs(UnmanagedType.Bool)] bool checked_);

    [LibraryImport("NotifyIconLibCpp.dll", EntryPoint = "SetEnabled")]
    private static partial void SetEnabled_(
        [MarshalAs(UnmanagedType.LPStr)] string guid,
        nint itemId,
        [MarshalAs(UnmanagedType.Bool)] bool enabled);

    #endregion
}
