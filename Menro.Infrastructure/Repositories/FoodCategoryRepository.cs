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
    public class FoodCategoryRepository : Repository<FoodCategory>, IFoodCategoryRepository
    {
        private readonly MenroDbContext _context;

        public FoodCategoryRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<FoodCategory>> GetAllByRestaurantAsync(int restaurantId)
        {
            return await _context.FoodCategories
                .Where(c => c.RestaurantId == restaurantId && !c.IsDeleted && c.IsAvailable)
                .ToListAsync();
        }

        public async Task<List<FoodCategory>> GetGlobalCategoriesAsync()
        {
            return await _context.FoodCategories
                .Where(c => c.GlobalFoodCategoryId != null && !c.IsDeleted && c.IsAvailable)
                .ToListAsync();
        }

        public async Task<List<FoodCategory>> GetAllFoodCategoriesForRestaurantAsync(string restaurantSlug)
        {
            var restaurant = await _context.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Slug == restaurantSlug);

            if (restaurant == null) return new List<FoodCategory>();

            var globalCategories = await _context.FoodCategories
                .Where(c => c.GlobalFoodCategoryId != null && !c.IsDeleted && c.IsAvailable)
                .AsNoTracking()
                .ToListAsync();

            var restaurantCategories = await _context.FoodCategories
                .Where(c => c.RestaurantId == restaurant.Id && !c.IsDeleted && c.IsAvailable)
                .AsNoTracking()
                .ToListAsync();

            return globalCategories.Concat(restaurantCategories).ToList();
        }

    }
}
