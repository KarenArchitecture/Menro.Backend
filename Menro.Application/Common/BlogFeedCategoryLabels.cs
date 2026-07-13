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
        private static readonly Dictionary<BlogPostSortOrder, string> Labels = new()
        {
            [BlogPostSortOrder.Newest] = "جدیدترین‌ها",
            [BlogPostSortOrder.MostPopular] = "محبوب‌ترین‌ها",
            [BlogPostSortOrder.MostViewed] = "پربازدیدترین‌ها",
        };

        public static string ToLabel(this BlogPostSortOrder category) =>
            Labels.TryGetValue(category, out var label) ? label : category.ToString();
    }
}
