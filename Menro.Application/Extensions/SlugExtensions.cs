using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Menro.Application.Extensions
{
    public static class SlugExtensions
    {
        /// <summary>
        /// Converts a Persian or mixed string into a SEO-friendly English slug
        /// </summary>
        public static string TransliterateToEnglish(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // 1. Map Persian letters to approximate English equivalents
            var map = new Dictionary<char, string>
            {
                ['ا'] = "a",
                ['ب'] = "b",
                ['پ'] = "p",
                ['ت'] = "t",
                ['ث'] = "s",
                ['ج'] = "j",
                ['چ'] = "ch",
                ['ح'] = "h",
                ['خ'] = "kh",
                ['د'] = "d",
                ['ذ'] = "z",
                ['ر'] = "r",
                ['ز'] = "z",
                ['ژ'] = "zh",
                ['س'] = "s",
                ['ش'] = "sh",
                ['ص'] = "s",
                ['ض'] = "z",
                ['ط'] = "t",
                ['ظ'] = "z",
                ['ع'] = "a",
                ['غ'] = "gh",
                ['ف'] = "f",
                ['ق'] = "gh",
                ['ک'] = "k",
                ['گ'] = "g",
                ['ل'] = "l",
                ['م'] = "m",
                ['ن'] = "n",
                ['و'] = "v",
                ['ه'] = "h",
                ['ی'] = "y",
                ['ء'] = "",
                ['ئ'] = "y",
                ['ؤ'] = "v"
            };

            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (map.ContainsKey(c))
                    sb.Append(map[c]);
                else if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else
                    sb.Append('-'); // replace spaces/punctuation with dash
            }

            // 2. Remove multiple consecutive dashes
            var slug = Regex.Replace(sb.ToString().ToLower(), @"-+", "-");

            // 3. Trim leading/trailing dashes
            return slug.Trim('-');
        }
    }
}
