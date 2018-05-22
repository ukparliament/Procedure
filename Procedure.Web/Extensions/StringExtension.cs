namespace Procedure.Web.Extensions
{
    public static class StringExtension
    {
        public static string RemoveQuotesAndTrim(this string str)
        {
            return str.Replace("\"", string.Empty).Trim();
        }

        public static string SurroundWithDoubleQuotes(this string str)
        {
            return "\"" + str + "\"";
        }

    }
}