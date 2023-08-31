using System.Runtime.InteropServices;

namespace ADLXWrapper;

public static partial class ADLXWrapper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FanRange
    {
        public int minTemperature;
        public int maxTemperature;
        public int minSpeed;
        public int maxSpeed;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FanSpeeds
    {
        public KeyValuePair<int, int> P1;
        public KeyValuePair<int, int> P2;
        public KeyValuePair<int, int> P3;
        public KeyValuePair<int, int> P4;
        public KeyValuePair<int, int> P5;
    }

    #region Public methods

    public static void Initialize() => Initialize_();

    public static int GetId(string pnpString) => GetId_(pnpString);

    public static List<int> GetGPUs()
    {
        GetGPUs_(out var items, out var _);
        return items.ToList();
    }

    public static bool IsSupported(int id) => IsSupported_(id);

    public static FanRange? GetFanRange(int id)
    {
        var pRange = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(FanRange)));
        if (!GetFanRange_(id, pRange))
        {
            return null;
        }

        return (FanRange?)Marshal.PtrToStructure(pRange, typeof(FanRange));
    }

    public static FanSpeeds? GetFanSpeeds(int id)
    {
        var pSpeeds = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(FanSpeeds)));
        if (!GetFanSpeeds_(id, pSpeeds))
        {
            return null;
        }

        return (FanSpeeds?)Marshal.PtrToStructure(pSpeeds, typeof(FanSpeeds));
    }

    public static void SetFanSpeeds(int id, FanSpeeds speeds)
    {
        var pSpeeds = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(FanSpeeds)));
        Marshal.StructureToPtr(speeds, pSpeeds, false);
        SetFanSpeeds_(id, pSpeeds);
    }

    public static void SetFanSpeed(int id, int speed) => SetFanSpeed_(id, speed);

    public static bool IsZeroRPMSupported(int id) => IsZeroRPMSupported_(id);

    public static bool IsZeroRPMEnabled(int id) => IsZeroRPMEnabled_(id);

    public static void SetZeroRPM(int id, bool enable) => SetZeroRPM_(id, enable);

    #endregion

    #region P/Invoke methods

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "Initialize")]
    private static partial void Initialize_();

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "GetId")]
    private static partial int GetId_([MarshalAs(UnmanagedType.LPStr)] string pnpString);

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "GetGPUs")]
    private static partial void GetGPUs_(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out int[] result,
        out int count);

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "IsSupported")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsSupported_(int id);

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "GetFanRange")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetFanRange_(int id, IntPtr result);

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "GetFanSpeeds")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetFanSpeeds_(int id, IntPtr result);

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "SetFanSpeeds")]
    private static partial void SetFanSpeeds_(int id, IntPtr speeds);

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "SetFanSpeed")]
    private static partial void SetFanSpeed_(int id, int speed);

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "IsZeroRPMSupported")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsZeroRPMSupported_(int id);

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "IsZeroRPMEnabled")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsZeroRPMEnabled_(int id);

    [LibraryImport("ADLXWrapperCpp.dll", EntryPoint = "SetZeroRPM")]
    private static partial void SetZeroRPM_(
        int id,
        [MarshalAs(UnmanagedType.Bool)] bool enable);

    #endregion
}
