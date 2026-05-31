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
        var existing = await _db.Icons.ToListAsync();

        foreach (var seed in IconSeedData.Data)
        {
            var item = existing.FirstOrDefault(x => x.FileName == seed.FileName);

            if (item == null)
            {
                await _db.Icons.AddAsync(seed);
            }
            else
            {
                item.Label = seed.Label;
            }
        }

        await _db.SaveChangesAsync();

        Console.WriteLine("[Seed] Icons synced.");
    }
}