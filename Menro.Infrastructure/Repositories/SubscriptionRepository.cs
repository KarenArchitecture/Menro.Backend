using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;

namespace Menro.Infrastructure.Repositories
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        private readonly MenroDbContext _context;

        public SubscriptionRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
