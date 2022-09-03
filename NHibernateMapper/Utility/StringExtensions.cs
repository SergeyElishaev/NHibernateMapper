using System.Globalization;
using System.Linq;

namespace NHibernateMapper.Utility
{
    public static class StringExtensions
    {
        public static string OnlyLetters(this string str)
        {
            return new string(str.Where(char.IsLetter).ToArray());
        }

        public static string ToTitleCase(this string str)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            //str = textInfo.ToTitleCase(str);

            if (str.Contains("_"))
            {
                str = str.Replace("_", "");
            }
            if (str.Contains(" "))
            {
                str = str.Replace(" ", "");
            }

            return str;
        }

        public static string Unwrap(this string str, char opening = '[', char closing = ']')
        {
            if (str.Contains(opening) && str.Contains(closing))
            {
                var indexOfOpening = str.IndexOf(opening);
                var indexOfClosing = str.IndexOf(closing);

                var result = str[++indexOfOpening..indexOfClosing];

                return result;
            }

            return str;
        }
    }
}
