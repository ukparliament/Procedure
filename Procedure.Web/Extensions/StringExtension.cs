using System.Linq;

namespace Procedure.Web.Extensions
{
    public static class StringExtension
    {
        public static string ProcessName(this string str)
        {
            string name = str.Replace("\"", string.Empty).Replace("[]", string.Empty).Trim();

            return string.Join(string.Empty, name.Split(' ').Select((c, i) => c + ((i + 1) % 3 == 0 ? "\\n" : " ")));
        }

        public static string SurroundWithDoubleQuotes(this string str)
        {
            return "\"" + str + "\"";
        }

    }
}