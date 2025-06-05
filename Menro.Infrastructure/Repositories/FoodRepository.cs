using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class FoodRepository : Repository<Food>, IFoodRepository
    {
        private readonly MenroDbContext _context;

        public FoodRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
