namespace Menro.Domain.Entities.Blog
{
    /// <summary>
    /// یک لایک از یه کاربر لاگین‌کرده روی یه پست. وجود یا عدم‌وجود این ردیف
    /// یعنی وضعیت لایک همون کاربر - BlogPost.LikeCount فقط یه شمارنده‌ی
    /// Cache‌شده‌ست که همراه با Add/Remove این جدول sync می‌مونه.
    /// </summary>
    public class BlogPostLike
    {
        public Guid Id { get; set; }
        public Guid BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }
    }
}