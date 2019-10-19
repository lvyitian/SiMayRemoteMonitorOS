using System.Text;

namespace SiMay.Core.Extensions
{

    public static class UnicodeExtensions
    {

        public static string ToUnicodeString(this byte[] data)
        {
            return Encoding.Unicode.GetString(data);
        }

        public static byte[] UnicodeStringToBytes(this string str)
        {
            return Encoding.Unicode.GetBytes(str);
        }
    }
}