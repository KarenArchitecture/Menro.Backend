using Menro.Domain.Entities.SiteContent;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Core.Seeders;

public class SiteLinkSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;

    public SiteLinkSeeder(MenroDbContext db)
    {
        _db = db;
    }

    public int Order => SeedOrder.SiteLink;

    public async Task SeedAsync()
    {
        // طبق درخواست: فقط اگر هیچ رکوردی توی کل جدول نیست سید کن، وگرنه هیچ کاری نکن
        if (await _db.SiteLinks.AnyAsync())
        {
            Console.WriteLine("[Seed] SiteLinks already exist, skipping.");
            return;
        }

        var links = new List<SiteLink>
        {
            // Header nav (لوگو جزو این لیست نیست — همچنان hardcoded توی AppHeader)
            new() { Id = Guid.NewGuid(), Location = MenuLocation.Header, Title = "وب‌اپ",     Url = "/home",          Order = 1, IsActive = true, ParentId = null },
            new() { Id = Guid.NewGuid(), Location = MenuLocation.Header, Title = "بلاگ",       Url = "/blog",          Order = 2, IsActive = true, ParentId = null },
            new() { Id = Guid.NewGuid(), Location = MenuLocation.Header, Title = "اشتراک‌ها",  Url = "/subscriptions", Order = 3, IsActive = true, ParentId = null },

            // Footer nav (سوشال‌ها جزو این لیست نیست)
            new() { Id = Guid.NewGuid(), Location = MenuLocation.Footer, Title = "وب اپ",      Url = "/home",          Order = 1, IsActive = true, ParentId = null },
            new() { Id = Guid.NewGuid(), Location = MenuLocation.Footer, Title = "اشتراک‌ها",  Url = "/subscriptions", Order = 2, IsActive = true, ParentId = null },
            new() { Id = Guid.NewGuid(), Location = MenuLocation.Footer, Title = "بلاگ",       Url = "/blog",          Order = 3, IsActive = true, ParentId = null },
            new() { Id = Guid.NewGuid(), Location = MenuLocation.Footer, Title = "رستوران‌ها", Url = "/restaurants",   Order = 4, IsActive = true, ParentId = null },
        };

        await _db.SiteLinks.AddRangeAsync(links);
        await _db.SaveChangesAsync();

        Console.WriteLine("[Seed] SiteLinks (header + footer nav) seeded.");
    }
}