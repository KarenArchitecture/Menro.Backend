using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Domain.Entities.Blog;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Menro.Infrastructure.Data.Seed.Demo.Seeders;

public class DemoBlogSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;
    private readonly IMediaStorageProvider _mediaStorage;
    private readonly MediaStorageOptions _mediaOptions;

    private readonly Random _rand = new(42);

    public DemoBlogSeeder(
        MenroDbContext db,
        IMediaStorageProvider mediaStorage,
        IOptions<MediaStorageOptions> mediaOptions)
    {
        _db = db;
        _mediaStorage = mediaStorage;
        _mediaOptions = mediaOptions.Value;
    }

    public int Order => SeedOrder.Blog;

    public async Task SeedAsync()
    {
        var alreadySeeded = await _db.Set<BlogPost>().AnyAsync();

        if (alreadySeeded)
        {
            Console.WriteLine("[Seed] Demo blog posts already seeded.");
            return;
        }

        // ------------------------------------------------------------
        // 1) Display categories (BlogPost.CategoryId is a required FK,
        //    so we need at least a handful of these to assign posts to)
        // ------------------------------------------------------------
        var categoryDefs = new (string Title, string Subtitle, string Slug, string ColorHex)[]
        {
            ("رستوران و فضای سرویس", "فضای فیزیکی، خدمات، جو", "restaurant-and-service-space", "#5A302F"),
            ("منو و غذا", "چیدمان، انتخاب، تجربه طعم", "menu-and-food", "#664A25"),
            ("رفتار و تجربه مشتری", "عادت‌ها، رضایت، وفاداری", "customer-behavior-and-experience", "#2B314B"),
            ("برند و بازاریابی", "ساخت برند، جذب، دیده‌شدن", "brand-and-marketing", "#274435"),
            ("مدیریت و عملیات", "پشت‌صحنه، منابع، فرآیندها", "management-and-operations", "#454C21"),
            ("تکنولوژی و ابزارها", "راهکارهای دیجیتال و هوشمند", "technology-and-tools", "#58273E"),
            ("فرهنگ و جامعه", "تأثیر اجتماعی، سبک زندگی", "culture-and-society", "#264648"),
            ("نگاه و دیدگاه", "تحلیل، ترند، زاویه‌ی متفاوت", "perspective-and-insight", "#41224D"),
        };

        var categories = categoryDefs
            .Select((c, i) => new BlogCategory
            {
                Id = Guid.NewGuid(),
                Title = c.Title,
                Subtitle = c.Subtitle,
                Slug = c.Slug,
                ColorHex = c.ColorHex,
                SortOrder = i,
                CreatedAtUtc = DateTime.UtcNow
            })
            .ToList();

        await _db.Set<BlogCategory>().AddRangeAsync(categories);
        await _db.SaveChangesAsync();

        // ------------------------------------------------------------
        // 2) Bulk blog posts - large volume for pagination testing
        // ------------------------------------------------------------
        var topics = new[]
        {
            "طرز تهیه پاستا آلفردو با مرغ و قارچ",
            "راز یک پیتزای ایتالیایی خوشمزه",
            "طرز تهیه قهوه دمی در خانه",
            "بهترین دسرها برای مهمانی‌های تابستانی",
            "آموزش پخت نان سیر رستورانی",
            "چگونه سالاد سزار حرفه‌ای درست کنیم؟",
            "بهترین رژیم غذایی برای ورزشکاران",
            "نکاتی برای نگهداری طولانی‌مدت سبزیجات",
            "۵ اشتباه رایج در پخت برنج",
            "طرز تهیه یک صبحانه مقوی و سریع",
            "ادویه‌های ضروری در هر آشپزخانه ایرانی",
            "تکنیک‌های طلایی برای مزه‌دار کردن جوجه کباب",
            "چگونه گوشت را سریع‌تر بپزیم؟",
            "آموزش گام به گام سوشی در خانه",
            "آشنایی با انواع پنیرها و کاربرد آنها",
            "فواید نوشیدن آب لیمو در صبح",
            "تفاوت قهوه عربیکا و روبوستا در چیست؟",
            "طرز تهیه همبرگر خانگی با گوشت تازه",
            "بررسی امکانات جدید اپلیکیشن منرو",
            "چالش‌های تغذیه سالم در زندگی کارمندی",
        };

        const int postCount = 500; // bump/lower as needed for your pagination testing

        // 🔧 Read the real sample bytes ONCE. Every post's cover still gets
        // saved individually below (entity-scoped by postId means a separate
        // physical file per post), so first run will do ~500 webp encodes —
        // a one-time startup cost, not a per-request cost. If that ever feels
        // too slow at seed time, say so and I'll cut postCount or share the
        // encoded output across posts instead of re-encoding per post.
        var blogBytes = File.ReadAllBytes(Path.Combine(_mediaOptions.RootPath, "media/img/blog/posts/blog.jpg"));

        var posts = new List<BlogPost>();

        for (int i = 1; i <= postCount; i++)
        {
            var topic = topics[_rand.Next(topics.Length)];
            var category = categories[_rand.Next(categories.Count)];

            // Spread creation dates out over the last ~2 years so "Newest"
            // sort has meaningful variety instead of near-identical timestamps.
            var createdAt = DateTime.UtcNow.AddDays(-_rand.Next(0, 730))
                                            .AddMinutes(-_rand.Next(0, 1440));

            var isPublished = _rand.Next(0, 100) < 85; // ~85% published, some drafts

            // BlogPost.Id is client-generated (Guid), so we know it before
            // insert and can use it as the media entityId right away.
            var postId = Guid.NewGuid();
            var coverResult = await _mediaStorage.SaveBytesAsync(MediaCategory.BlogPostImage, blogBytes, ".jpg", postId.ToString());

            posts.Add(new BlogPost
            {
                Id = postId,
                Title = $"{topic} - شماره {i}",
                CoverImageUrl = coverResult.FileName,
                ReadingMinutes = _rand.Next(2, 12),
                CategoryId = category.Id,
                IsPublished = isPublished,
                CreatedAtUtc = createdAt,
                UpdatedAtUtc = _rand.Next(0, 2) == 0
                    ? createdAt.AddDays(_rand.Next(1, 10))
                    : null,
                ViewCount = _rand.Next(0, 20_000),
                LikeCount = _rand.Next(0, 3_000),
            });
        }

        await _db.Set<BlogPost>().AddRangeAsync(posts);
        await _db.SaveChangesAsync();

        Console.WriteLine(
            $"[Seed] {categories.Count} blog categories and {posts.Count} demo blog posts seeded.");
    }
}