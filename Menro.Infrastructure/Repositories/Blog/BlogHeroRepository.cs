using Menro.Domain.Entities.Blog;
using Menro.Domain.Interfaces.Blog;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class BlogHeroRepository : IBlogHeroRepository
    {
        private readonly MenroDbContext _context;

        public BlogHeroRepository(MenroDbContext context)
        {
            _context = context;
        }

        /// <summary>There is only ever one hero row; returns it (or null if never seeded).</summary>
        public async Task<BlogHero?> GetAsync(CancellationToken ct = default)
        {
            return await _context.BlogHeroes.FirstOrDefaultAsync(ct);
        }

        /// <summary>Creates the singleton row on first save, updates it on every save after.</summary>
        public async Task<BlogHero> UpsertAsync(BlogHero hero, CancellationToken ct = default)
        {
            var existing = await _context.BlogHeroes.FirstOrDefaultAsync(ct);

            if (existing is null)
            {
                hero.Id = hero.Id == Guid.Empty ? Guid.NewGuid() : hero.Id;
                hero.UpdatedAtUtc = DateTime.UtcNow;
                await _context.BlogHeroes.AddAsync(hero, ct);
            }
            else
            {
                existing.TitleLine = hero.TitleLine;
                existing.Highlight = hero.Highlight;
                existing.SearchPlaceholder = hero.SearchPlaceholder;
                existing.UpdatedAtUtc = DateTime.UtcNow;
                hero = existing;
            }

            await _context.SaveChangesAsync(ct);
            return hero;
        }
    }
}
