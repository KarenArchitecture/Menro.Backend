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
        // ⚠️ DEV-ONLY: این seeder دیگه چک نمی‌کنه که قبلاً سید شده یا نه -
        // هر بار InitializeAsync اجرا بشه، پست‌ها و categoryهای قبلی بلاگ
        // پاک می‌شن و از نو ساخته می‌شن. برای دیباگ/تست خوبه، ولی روی محیطی
        // که دیتای واقعی توش هست هرگز اجرا نکن.
        var existingPosts = await _db.Set<BlogPost>().ToListAsync();
        if (existingPosts.Count > 0)
        {
            _db.Set<BlogPost>().RemoveRange(existingPosts);
            await _db.SaveChangesAsync();
        }

        var existingCategories = await _db.Set<BlogCategory>().ToListAsync();
        if (existingCategories.Count > 0)
        {
            _db.Set<BlogCategory>().RemoveRange(existingCategories);
            await _db.SaveChangesAsync();
        }

        // ------------------------------------------------------------
        // 1) Display categories (BlogPost.CategoryId is now a nullable
        //    FK, so a handful of posts will intentionally be left
        //    uncategorized below to cover that case in pagination/filter
        //    testing)
        // ------------------------------------------------------------
        // NOTE: BlogCategory has no AuthorId/AuthorNameSnapshot fields
        // (it's a display card, not authored content), so none are set here.
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
        // 2) Demo authors (name-only snapshots)
        // ------------------------------------------------------------
        // NOTE: BlogPost.AuthorId points at Menro.Domain.Entities... User,
        // but there's no User seeder wired in here, so AuthorId is left
        // null for all demo posts. AuthorNameSnapshot is filled from this
        // list purely for display/testing purposes. If you have a demo
        // user seeder (or want real AuthorId FKs), send it over and I'll
        // wire it in properly instead of leaving AuthorId null.
        var authorNames = new[]
        {
            "سارا احمدی",
            "علی محمدی",
            "نگار حسینی",
            "امیر رضایی",
            "مهسا کریمی",
        };

        // ------------------------------------------------------------
        // 3) Bulk blog posts
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

        const int postCount = 10; // 👈 برای تست/دیباگ کم شد

        var blogBytes = File.ReadAllBytes(Path.Combine(_mediaOptions.RootPath, "media/img/blog/posts/blog.jpg"));

        var posts = new List<BlogPost>();

        for (int i = 1; i <= postCount; i++)
        {
            var topic = topics[_rand.Next(topics.Length)];

            var category = _rand.Next(0, 100) < 5
                ? null
                : categories[_rand.Next(categories.Count)];

            var authorName = authorNames[_rand.Next(authorNames.Length)];
            var slug = $"{Slugify(topic)}-{i}";

            var createdAt = DateTime.UtcNow.AddDays(-_rand.Next(0, 730))
                                            .AddMinutes(-_rand.Next(0, 1440));

            var isPublished = _rand.Next(0, 100) < 85;

            var postId = Guid.NewGuid();
            var coverResult = await _mediaStorage.SaveBytesAsync(MediaCategory.BlogPostImage, blogBytes, ".jpg", postId.ToString());

            posts.Add(new BlogPost
            {
                Id = postId,
                Title = $"{topic} - شماره {i}",
                Slug = slug,
                AuthorId = null,
                AuthorNameSnapshot = authorName,
                CoverImageUrl = coverResult.FileName,
                ReadingMinutes = _rand.Next(2, 12),
                CategoryId = category?.Id,
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

    private static string Slugify(string text)
    {
        return "post-" + Guid.NewGuid().ToString("N")[..8];
    }
}