// http://www.pinvoke.net/default.aspx/Structures/WINDOWPLACEMENT.html
using System.Runtime.InteropServices;

namespace Anemos.Helpers.PInvoke;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
internal struct WINDOWPLACEMENT
{
    public uint Length;
    public uint Flags;
    public uint ShowCmd;
    public POINT MinPosition;
    public POINT MaxPosition;
    public RECT NormalPosition;

    public static WINDOWPLACEMENT Default
    {
        get
        {
            var result = new WINDOWPLACEMENT();
            result.Length = (uint)Marshal.SizeOf(result);
            return result;
        }
    }
}
