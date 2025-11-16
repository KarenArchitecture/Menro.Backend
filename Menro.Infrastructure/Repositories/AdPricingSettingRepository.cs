using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;

namespace Menro.Infrastructure.Repositories
{
    public class AdPricingSettingRepository : IAdPricingSettingRepository
    {
        private readonly MenroDbContext _context;

        public AdPricingSettingRepository(MenroDbContext context)
        {
            _context = context;
        }
    }
}
