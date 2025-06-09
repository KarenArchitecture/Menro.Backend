using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantCategoryRepository : Repository<RestaurantCategory>, IRestaurantCategoryRepository
    {
        private readonly MenroDbContext _context;

        public RestaurantCategoryRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
