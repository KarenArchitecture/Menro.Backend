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

        /// <summary>
        /// Returns all restaurant-specific (Custom) food categories for a given restaurant.
        /// These are categories created by the restaurant owner.
        /// </summary>
        /// <param name="restaurantId">The ID of the restaurant</param>
        /// <returns>List of CustomFoodCategory objects</returns>
        public async Task<List<CustomFoodCategory>> GetAllByRestaurantAsync(int restaurantId)
        {
            // Fetch all CustomFoodCategory records where the RestaurantId matches.
            // Removed checks for IsDeleted & IsAvailable since those properties no longer exist.
            // Ordering by Name for consistent presentation in UI.
            return await _context.FoodCategories
                .Where(c => c.RestaurantId == restaurantId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Returns all global food categories.
        /// These are admin-defined categories that are available to all restaurants.
        /// </summary>
        /// <returns>List of GlobalFoodCategory objects</returns>
        public async Task<List<GlobalFoodCategory>> GetGlobalCategoriesAsync()
        {
            // Fetch all GlobalFoodCategory records from the database.
            // Ordering by DisplayOrder allows admin to control the presentation order.
            return await _context.GlobalFoodCategories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }


        /// <summary>
        /// Returns both global categories and restaurant-specific categories for a given restaurant.
        /// Useful for building the full menu filter in restaurant pages.
        /// </summary>
        /// <param name="restaurantSlug">The slug (unique URL name) of the restaurant</param>
        /// <returns>Combined list of Global + Custom food categories</returns>
        public async Task<List<object>> GetAllFoodCategoriesForRestaurantAsync(string restaurantSlug)
        {
            // 1️⃣ Find the restaurant by its slug
            var restaurant = await _context.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Slug == restaurantSlug);

            if (restaurant == null)
                return new List<object>();

            // 2️⃣ Fetch all global categories
            var globalCategories = await _context.GlobalFoodCategories
                .OrderBy(c => c.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

            // 3️⃣ Fetch all restaurant-specific categories
            var restaurantCategories = await _context.FoodCategories
                .Where(c => c.RestaurantId == restaurant.Id)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            // 4️⃣ Combine both lists into one
            return globalCategories.Cast<object>()
                .Concat(restaurantCategories.Cast<object>())
                .ToList();
        }


    }
}
