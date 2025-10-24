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


        //                                      == Restaurant Page ==
        /// <summary>
        /// Get all active global categories (admin-defined) ordered by DisplayOrder.
        /// </summary>
        public async Task<List<GlobalFoodCategory>> GetActiveGlobalCategoriesAsync()
        {
            return await _context.GlobalFoodCategories
                .AsNoTracking()
                .Where(gc => gc.IsActive)
                .OrderBy(gc => gc.DisplayOrder)
                .ToListAsync();
        }

        /// <summary>
        /// Get all available (not deleted) custom categories for a specific restaurant.
        /// </summary>
        public async Task<List<CustomFoodCategory>> GetAvailableCustomCategoriesForRestaurantAsync(int restaurantId)
        {
            return await _context.CustomFoodCategories
                .AsNoTracking()
                .Where(c => c.RestaurantId == restaurantId && c.IsAvailable && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

    }
}
