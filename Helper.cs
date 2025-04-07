using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace rename
{
    public static class Helper
    {
        public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = SelectedSerializerContext.Default
        }; 
        public static DateTime Trim(this DateTime date, long roundTicks)
        {
            return new DateTime(date.Ticks - date.Ticks % roundTicks, date.Kind);
        }
        public static string ToSha256HexHashString(this string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            var hex = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return hex;
        }
    }
}