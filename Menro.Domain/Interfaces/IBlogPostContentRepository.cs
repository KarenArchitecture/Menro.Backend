using Menro.Domain.Entities.Blog;

namespace Menro.Domain.Interfaces
{
    public interface IBlogPostContentRepository
    {
        /// <summary>فقط خود محتوا - برای صفحه‌ی ادیتور Tiptap کافیه.</summary>
        Task<BlogPostContent?> GetByPostIdAsync(Guid postId, CancellationToken ct = default);

        /// <summary>محتوا + BlogPost (و Category) - برای رندر صفحه‌ی عمومی تک‌پست
        /// که هم متادیتا هم بدنه‌ی HTML رو با یه رفت‌وبرگشت لازم داره.</summary>
        Task<BlogPostContent?> GetByPostIdWithPostAsync(Guid postId, CancellationToken ct = default);

        Task AddAsync(BlogPostContent content, CancellationToken ct = default);
        Task UpdateAsync(BlogPostContent content, CancellationToken ct = default);
        // بدون DeleteAsync: با حذف BlogPost، ردیف Content هم به‌واسطه‌ی
        // Cascade (تو BlogPostContentConfiguration) خودکار پاک میشه.
        // بدون GetAllAsync: هیچ صفحه‌ای همه‌ی محتواها رو یهو نیاز نداره.
    }
}