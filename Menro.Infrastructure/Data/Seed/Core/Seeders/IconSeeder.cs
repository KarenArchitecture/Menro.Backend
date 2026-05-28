using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Menro.Infrastructure.Data.Seed.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Core.Seeders;

public class IconSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;

    public IconSeeder(MenroDbContext db)
    {
        _db = db;
    }
    public int Order => SeedOrder.Icon;
    public async Task SeedAsync()
    {
        if (await _db.Icons.AnyAsync())
        {
            Console.WriteLine("[Seed] Icons already seeded.");
            return;
        }

        await _db.Icons.AddRangeAsync(
            IconSeedData.Data);

        await _db.SaveChangesAsync();

        Console.WriteLine(
            $"[Seed] {IconSeedData.Data.Count} icons seeded.");
    }
}