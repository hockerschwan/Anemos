using System.Runtime.InteropServices;
using System.Text;

namespace Anemos.Helpers;

public static partial class RuntimeHelper
{
    public static bool DestoyIcon(IntPtr hIcon) => DestroyIcon_(hIcon);

    public static System.Drawing.Icon? ExtractIcon(string file, int number, bool largeIcon = false)
    {
        ExtractIconEx_(file, number, out var large, out var small, 1);
        try
        {
            return System.Drawing.Icon.FromHandle(largeIcon ? large : small);
        }
        catch
        {
            return null;
        }
    }

    public static uint GetDpiForWindow(WindowEx window)
    {
        return GetDpiForWindow_(window.GetWindowHandle());
    }

    public static IntPtr GetModuleHandle(string? module = null)
    {
        return module == null ? GetModuleHandle_() : GetModuleHandle_(module);
    }

    public static PInvoke.RECT GetWindowRect(WindowEx window)
    {
        if (GetWindowRect_(window.GetWindowHandle(), out var res))
        {
            return res;
        }
        throw new Exception("GetWindowRect failed.");
    }

    public static bool IsMaximized(WindowEx window)
    {
        var placement = new PInvoke.WINDOWPLACEMENT();
        if (GetWindowPlacement_(window.GetWindowHandle(), ref placement))
        {
            return placement.ShowCmd == (uint)PInvoke.ShowWindowCommands.Maximize;
        }
        throw new Exception("GetWindowPlacement failed.");
    }

    public static bool IsMinimized(WindowEx window)
    {
        var placement = new PInvoke.WINDOWPLACEMENT();
        if (GetWindowPlacement_(window.GetWindowHandle(), ref placement))
        {
            return placement.ShowCmd == (uint)PInvoke.ShowWindowCommands.ShowMinimized;
        }
        throw new Exception("GetWindowPlacement failed.");
    }

    public static bool IsMSIX
    {
        get
        {
            var length = 0;
            return GetCurrentPackageFullName_(ref length, null) != 15700L;
        }
    }
    public static IntPtr LoadIcon(string? module, int id)
    {
        return LoadIcon_(GetModuleHandle(module), id);
    }

    ////////////////////

    [LibraryImport("user32.dll", EntryPoint = "DestroyIcon")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyIcon_(IntPtr handle);

    [LibraryImport("Shell32.dll", EntryPoint = "ExtractIconExW", StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    private static partial int ExtractIconEx_(string file, int index, out IntPtr large, out IntPtr small, int numIcons);

    [DllImport("kernel32.dll", EntryPoint = "GetCurrentPackageFullName", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName_(ref int packageFullNameLength, StringBuilder? packageFullName);

    [LibraryImport("user32.dll", EntryPoint = "GetDpiForWindow", SetLastError = false)]
    private static partial uint GetDpiForWindow_(IntPtr hWnd);

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true)]
    public static partial IntPtr GetModuleHandle_();

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr GetModuleHandle_([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowPlacement", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowPlacement_(IntPtr hWnd, ref PInvoke.WINDOWPLACEMENT lpwndpl);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowRect", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect_(IntPtr hwnd, out PInvoke.RECT lpRect);

    [LibraryImport("user32.dll", EntryPoint = "LoadIconW")]
    private static partial IntPtr LoadIcon_(IntPtr hInstance, IntPtr lpIconName);
}
