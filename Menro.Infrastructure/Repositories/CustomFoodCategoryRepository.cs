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
    public class CustomFoodCategoryRepository : Repository<CustomFoodCategory>, ICustomFoodCategoryRepository
    {
        private readonly MenroDbContext _context;

        public CustomFoodCategoryRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomFoodCategory>> GetByRestaurantSlugAsync(string restaurantSlug)
        {
            return await _context.CustomFoodCategory
                .Where(fc => fc.Restaurant.Slug == restaurantSlug)
                .ToListAsync();
        }
        public async Task<bool> CreateAsync(CustomFoodCategory category)
        {
            try
            {
                await _context.CustomFoodCategory.AddAsync(category);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<IEnumerable<CustomFoodCategory>> GetCustomFoodCategoriesAsync(int restaurantId)
        {
            return await _context.CustomFoodCategory.Where(u => u.RestaurantId == restaurantId).ToListAsync();
        }

    }
}
