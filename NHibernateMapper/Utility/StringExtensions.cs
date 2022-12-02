using System.Linq;
using System.Text;

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
            var parts = str.Split('_');

            var result = new StringBuilder();
            foreach (var part in parts)
            {
                if (part.Length > 0)
                {
                    string titleCasedWord = char.ToUpper(part[0]).ToString();
                    for (int j = 1; j < part.Length; j++)
                    {
                        titleCasedWord += char.ToLower(part[j]);
                    }
                    result.Append(titleCasedWord);
                }
            }

            return result.ToString();
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

        public static string Wrap(this string str, char opening = '[', char closing = ']')
        {
            return $"{opening}{str}{closing}";
        }
    }
}
