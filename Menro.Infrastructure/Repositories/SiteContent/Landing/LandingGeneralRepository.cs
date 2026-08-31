using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Menro.Domain.Entities.SiteContent;
using Menro.Domain.Interfaces.SiteContent;

namespace Menro.Infrastructure.Repositories.SiteContent
{
    public class LandingGeneralRepository : ILandingGeneralRepository
    {
        private readonly MenroDbContext _context;

        public LandingGeneralRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task<LandingGeneral> GetOrCreateAsync()
        {
            var entity = await _context.LandingGeneral.FirstOrDefaultAsync();
            if (entity is null)
            {
                entity = new LandingGeneral
                {
                    Id = LandingGeneral.SingletonId,
                    HeroHighlight = "منرو",
                    HeroTitle = "بهترین همیار رستوران تو",
                    SpotlightTitle = "با منرو تو چشم باش",
                    UpdatedAtUtc = DateTime.UtcNow,
                };
                _context.LandingGeneral.Add(entity);
                await _context.SaveChangesAsync();
            }

            return entity;
        }

        public async Task UpdateAsync(LandingGeneral entity)
        {
            _context.LandingGeneral.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
