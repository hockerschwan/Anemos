using System.Text;

namespace Anemos.Helpers;

public static class Helper
{
    public static Guid GenerateGuid(string str)
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
}
