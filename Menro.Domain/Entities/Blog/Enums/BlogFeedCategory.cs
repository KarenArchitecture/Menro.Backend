namespace Menro.Domain.Enums
{
    /// <summary>
    /// The feed filter pills shown above the blog feed (جدیدترین‌ها / محبوب‌ترین‌ها / ...).
    /// Per product decision, these are NOT dynamic/editable from the admin panel anymore,
    /// so they live as a fixed enum instead of a database-backed table.
    /// Every BlogPost must belong to exactly one of these.
    /// </summary>
    public enum BlogFeedCategory
    {
        Newest = 1,       // جدیدترین‌ها
        MostPopular = 2,  // محبوب‌ترین‌ها
        MostViewed = 3,   // پربازدیدترین‌ها
        Trending = 4      // داغ‌ترین‌ها
    }
}
