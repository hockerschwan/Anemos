using System.Runtime.InteropServices;

namespace NotifyIconLib.Native;

internal partial class NativeFunctions
{
    #region Internal

    [StructLayout(LayoutKind.Sequential)]
    internal struct MenuItemStruct_
    {
        public uint Id;
        public MenuItemType Type;
        [MarshalAs(UnmanagedType.LPWStr)] public string Text = string.Empty;
        [MarshalAs(UnmanagedType.Bool)] public bool IsChecked;
        [MarshalAs(UnmanagedType.Bool)] public bool IsEnabled = true;
        public IntPtr hIcon;

        public MenuItemStruct_(
            uint id,
            MenuItemType type = 0,
            string text = "",
            bool isChecked = false,
            bool isEnabled = true)
        {
            Id = id;
            Type = type;
            Text = text;
            IsChecked = isChecked;
            IsEnabled = isEnabled;
        }
    }

    internal delegate void CallbackVoid();
    internal delegate void CallbackGuid([MarshalAs(UnmanagedType.LPStr)] string guid);
    internal delegate void CallbackUint([MarshalAs(UnmanagedType.LPStr)] string guid, uint value);

    internal static void CreateNotifyIcon(Guid guid, System.Drawing.Icon icon)
    {
        var hicon = icon.Handle;
        CreateNotifyIcon_(guid.ToString().ToLower(), hicon);
    }

    internal static void DeleteNotifyIcon(Guid guid)
    {
        DeleteNotifyIcon_(guid.ToString().ToLower());
    }

    internal static void SetCallback_IconClick(CallbackGuid callback)
    {
        SetCallback_IconClick_(Marshal.GetFunctionPointerForDelegate(callback));
    }

    internal static void SetCallback_ItemClick(CallbackUint callback)
    {
        SetCallback_ItemClick_(Marshal.GetFunctionPointerForDelegate(callback));
    }

    internal static void SetIcon(Guid guid, ref System.Drawing.Icon icon)
    {
        SetIcon_(guid.ToString().ToLower(), icon.Handle);
    }

    internal static void SetMenuItems(Guid guid, IList<MenuItemStruct_> menuItems)
    {
        var ptrArr = new IntPtr[menuItems.Count];
        for (var i = 0; i < menuItems.Count; ++i)
        {
            ptrArr[i] = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(MenuItemStruct_)));
            Marshal.StructureToPtr(menuItems[i], ptrArr[i], false);
        }
        SetMenuItems_(guid.ToString().ToLower(), ptrArr.Length, ptrArr);
    }

    internal static void SetTooltip(Guid guid, string tooltip)
    {
        SetTooltip_(guid.ToString().ToLower(), tooltip);
    }

    internal static void SetVisibility(Guid guid, bool visible)
    {
        SetVisibility_(guid.ToString().ToLower(), visible);
    }

    internal static void SetChecked(Guid guid, uint itemId, bool checked_)
    {
        SetChecked_(guid.ToString().ToLower(), (nint)itemId, checked_);
    }

    internal static void SetEnabled(Guid guid, uint itemId, bool enabled)
    {
        SetEnabled_(guid.ToString().ToLower(), (nint)itemId, enabled);
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
        int count, IntPtr[] items);

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
