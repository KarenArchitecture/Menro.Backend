using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    public class FoodCategoryRepository : Repository<CustomFoodCategory>, IFoodCategoryRepository
    {
        private readonly MenroDbContext _context;

        public FoodCategoryRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomFoodCategory>> GetByRestaurantSlugAsync(string restaurantSlug)
        {
            return await _context.FoodCategories
                .Where(fc => fc.Restaurant.Slug == restaurantSlug)
                .ToListAsync();
        }
    }
}
