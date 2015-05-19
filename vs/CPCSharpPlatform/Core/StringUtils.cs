namespace CP.Core
{
    public static class StringUtils
    {
        public static string Concat(string str1, string str2)
        {
            return str1 + str2;
        }

        public static string Substring(string str, int startIndex)
        {
            return str.Substring(startIndex);
        }

        public static string Substring(string str, int startIndex, int length)
        {
            return str.Substring(startIndex, length);
        }
    }
}