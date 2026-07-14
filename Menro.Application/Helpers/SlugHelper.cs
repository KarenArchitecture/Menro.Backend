using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Menro.Application.Helpers
{
    /// <summary>
    /// Generates URL-friendly, Latin-only slugs from Persian (or mixed) text,
    /// e.g. "روزانه مطالعات" -> "rouzaneh-motalaat".
    ///
    /// This is a best-effort informal ("Finglish") transliteration, not a
    /// linguistically perfect one - Persian has several letters that map to
    /// the same Latin sound (e.g. ث/س/ص all -> "s"), which is expected and
    /// fine for slugs. Vowels are approximated since Persian script normally
    /// omits short vowels entirely.
    /// </summary>
    public static class SlugHelper
    {
        // Ordered by longest-match-first where relevant (e.g. digraphs before
        // their single-letter components) so Replace-based lookup is safe.
        private static readonly (string Persian, string Latin)[] Map =
        {
            ("آ", "a"), ("ا", "a"), ("ب", "b"), ("پ", "p"), ("ت", "t"),
            ("ث", "s"), ("ج", "j"), ("چ", "ch"), ("ح", "h"), ("خ", "kh"),
            ("د", "d"), ("ذ", "z"), ("ر", "r"), ("ز", "z"), ("ژ", "zh"),
            ("س", "s"), ("ش", "sh"), ("ص", "s"), ("ض", "z"), ("ط", "t"),
            ("ظ", "z"), ("ع", "a"), ("غ", "gh"), ("ف", "f"), ("ق", "gh"),
            ("ک", "k"), ("ك", "k"), ("گ", "g"), ("ل", "l"), ("م", "m"),
            ("ن", "n"), ("و", "v"), ("ه", "h"), ("ة", "h"), ("ی", "y"),
            ("ي", "y"), ("ء", ""), ("ئ", "y"), ("أ", "a"), ("إ", "e"),
            ("‌", "-"), // ZWNJ (نیم‌فاصله) -> hyphen
            ("۰", "0"), ("۱", "1"), ("۲", "2"), ("۳", "3"), ("۴", "4"),
            ("۵", "5"), ("۶", "6"), ("۷", "7"), ("۸", "8"), ("۹", "9"),
        };

        private static readonly Regex InvalidCharsRegex = new(@"[^a-z0-9\-]", RegexOptions.Compiled);
        private static readonly Regex MultiDashRegex = new(@"-{2,}", RegexOptions.Compiled);

        /// <summary>
        /// Transliterates Persian text to Latin and slugifies it
        /// (lowercase, spaces/underscores -> hyphens, invalid chars stripped,
        /// no leading/trailing/duplicate hyphens). Returns "n-a" if the
        /// result would otherwise be empty (e.g. input was pure punctuation).
        /// </summary>
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "n-a";

            var sb = new StringBuilder(input.Trim());

            foreach (var (persian, latin) in Map)
                sb.Replace(persian, latin);

            var text = sb.ToString().ToLowerInvariant();

            // Normalize any remaining diacritics on Latin characters (é -> e, etc.)
            text = RemoveDiacritics(text);

            // Whitespace and underscores -> hyphen
            text = Regex.Replace(text, @"[\s_]+", "-");

            // Strip anything that isn't a-z, 0-9, or hyphen
            text = InvalidCharsRegex.Replace(text, "");

            // Collapse multiple hyphens and trim leading/trailing ones
            text = MultiDashRegex.Replace(text, "-").Trim('-');

            return string.IsNullOrEmpty(text) ? "n-a" : text;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
