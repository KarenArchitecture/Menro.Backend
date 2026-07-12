using Menro.Domain.Enums;

namespace Menro.Application.Common
{
    /// <summary>
    /// Persian display labels for the fixed feed categories. Since these are no
    /// longer editable/dynamic, this static map replaces what used to be the
    /// "feed-tags" table.
    /// </summary>
    public static class BlogFeedCategoryLabels
    {
        private static readonly Dictionary<BlogFeedCategory, string> Labels = new()
        {
            [BlogFeedCategory.Newest] = "جدیدترین‌ها",
            [BlogFeedCategory.MostPopular] = "محبوب‌ترین‌ها",
            [BlogFeedCategory.MostViewed] = "پربازدیدترین‌ها",
            [BlogFeedCategory.Trending] = "داغ‌ترین‌ها",
        };

        public static string ToLabel(this BlogFeedCategory category) =>
            Labels.TryGetValue(category, out var label) ? label : category.ToString();
    }
}
