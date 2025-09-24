// Menro.Infrastructure/Repositories/FoodRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class FoodRepository : IFoodRepository
    {
        private readonly MenroDbContext _context;

        public FoodRepository(MenroDbContext context)
        {
            _context = context;
        }

        /* ===================== Home Page (GLOBAL categories) ===================== */

        public async Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesAsync()
        {
            // Only active global categories that have at least one eligible food
            // (active restaurant, not deleted, etc.)
            var nowActiveGlobals = await _context.GlobalFoodCategories
                .AsNoTracking()
                .Where(gc => gc.IsActive)
                .ToListAsync();

            // Optional: pre-filter to those that actually have foods available
            // for better UX (avoid returning empty rows).
            var eligibleGlobalIds = await _context.Foods
                .AsNoTracking()
                .Include(f => f.Restaurant)
                .Include(f => f.FoodCategory)
                .Where(f =>
                    f.IsAvailable && !f.IsDeleted &&
                    f.Restaurant.IsActive && f.Restaurant.IsApproved &&
                    f.FoodCategory.IsAvailable && !f.FoodCategory.IsDeleted &&
                    f.FoodCategory.GlobalFoodCategoryId != null)
                .Select(f => f.FoodCategory.GlobalFoodCategoryId!.Value)
                .Distinct()
                .ToListAsync();

            return nowActiveGlobals
                .Where(gc => eligibleGlobalIds.Contains(gc.Id))
                .OrderBy(gc => gc.DisplayOrder)
                .ThenBy(gc => gc.Name)
                .ToList();
        }

        public async Task<List<int>> GetAllGlobalCategoryIdsAsync()
        {
            return await _context.GlobalFoodCategories
                .AsNoTracking()
                .Where(gc => gc.IsActive)
                .Select(gc => gc.Id)
                .ToListAsync();
        }

        public async Task<List<GlobalFoodCategory>> GetAllGlobalCategoriesExcludingAsync(List<string> excludeTitles)
        {
            excludeTitles ??= new List<string>();

            var globals = await _context.GlobalFoodCategories
                .AsNoTracking()
                .Where(gc => gc.IsActive && !excludeTitles.Contains(gc.Name))
                .ToListAsync();

            // Keep only those with eligible foods
            var eligibleGlobalIds = await _context.Foods
                .AsNoTracking()
                .Include(f => f.Restaurant)
                .Include(f => f.FoodCategory)
                .Where(f =>
                    f.IsAvailable && !f.IsDeleted &&
                    f.Restaurant.IsActive && f.Restaurant.IsApproved &&
                    f.FoodCategory.IsAvailable && !f.FoodCategory.IsDeleted &&
                    f.FoodCategory.GlobalFoodCategoryId != null)
                .Select(f => f.FoodCategory.GlobalFoodCategoryId!.Value)
                .Distinct()
                .ToListAsync();

            return globals
                .Where(gc => eligibleGlobalIds.Contains(gc.Id))
                .OrderBy(gc => gc.DisplayOrder)
                .ThenBy(gc => gc.Name)
                .ToList();
        }

        public async Task<List<Food>> GetPopularFoodsByGlobalCategoryIdAsync(int globalCategoryId, int count)
        {
            // order by server-side stats, take a small set, then include navs for just those rows
            var topIds = await _context.Foods
                .AsNoTracking()
                .Where(f =>
                    f.FoodCategory.GlobalFoodCategoryId == globalCategoryId &&
                    f.IsAvailable && !f.IsDeleted &&
                    f.Restaurant.IsActive && f.Restaurant.IsApproved &&
                    f.FoodCategory.IsAvailable && !f.FoodCategory.IsDeleted)
                .Select(f => new
                {
                    f.Id,
                    Avg = _context.FoodRatings
                            .Where(r => r.FoodId == f.Id)
                            .Select(r => (double?)r.Score)
                            .Average() ?? 0.0,
                    Voters = _context.FoodRatings.Count(r => r.FoodId == f.Id)
                })
                .OrderByDescending(x => x.Avg)
                .ThenByDescending(x => x.Voters)
                .Take(count)
                .Select(x => x.Id)
                .ToListAsync();

            return await _context.Foods
                .AsNoTracking()
                .Where(f => topIds.Contains(f.Id))
                .Include(f => f.Restaurant)    // minimal
                .ToListAsync();
        }

        /* ===================== Home Page – restaurant-local categories (kept) ===================== */

        public async Task<List<FoodCategory>> GetAllCategoriesAsync()
        {
            return await _context.FoodCategories
                .AsNoTracking()
                .Include(c => c.Restaurant)
                .Where(c =>
                    c.IsAvailable && !c.IsDeleted &&
                    c.Restaurant.IsActive && c.Restaurant.IsApproved)
                .ToListAsync();
        }

        public async Task<List<int>> GetAllCategoryIdsAsync()
        {
            return await _context.FoodCategories
                .AsNoTracking()
                .Include(c => c.Restaurant)
                .Where(c =>
                    c.IsAvailable && !c.IsDeleted &&
                    c.Restaurant.IsActive && c.Restaurant.IsApproved)
                .Select(fc => fc.Id)
                .ToListAsync();
        }

        public async Task<List<FoodCategory>> GetAllCategoriesExcludingAsync(List<string> excludeTitles)
        {
            excludeTitles ??= new List<string>();

            return await _context.FoodCategories
                .AsNoTracking()
                .Include(c => c.Restaurant)
                .Where(c =>
                    !excludeTitles.Contains(c.Name) &&
                    c.IsAvailable && !c.IsDeleted &&
                    c.Restaurant.IsActive && c.Restaurant.IsApproved)
                .ToListAsync();
        }

        public async Task<List<Food>> GetPopularFoodsByCategoryAsync(int categoryId, int count = 8)
        {
            var topIds = await _context.Foods
                .AsNoTracking()
                .Where(f =>
                    f.FoodCategoryId == categoryId &&
                    f.IsAvailable && !f.IsDeleted &&
                    f.Restaurant.IsActive && f.Restaurant.IsApproved &&
                    f.FoodCategory.IsAvailable && !f.FoodCategory.IsDeleted)
                .Select(f => new
                {
                    f.Id,
                    Avg = _context.FoodRatings.Where(r => r.FoodId == f.Id)
                                              .Select(r => (double?)r.Score).Average() ?? 0.0,
                    Voters = _context.FoodRatings.Count(r => r.FoodId == f.Id)
                })
                .OrderByDescending(x => x.Avg)
                .ThenByDescending(x => x.Voters)
                .Take(count)
                .Select(x => x.Id)
                .ToListAsync();

            return await _context.Foods
                .AsNoTracking()
                .Where(f => topIds.Contains(f.Id))
                .Include(f => f.Restaurant)
                .ToListAsync();
        }

        public async Task<List<Food>> GetPopularFoodsByCategoryIdAsync(int categoryId, int take)
        {
            var topIds = await _context.Foods
                .AsNoTracking()
                .Where(f =>
                    f.FoodCategoryId == categoryId &&
                    f.IsAvailable && !f.IsDeleted &&
                    f.Restaurant.IsActive && f.Restaurant.IsApproved &&
                    f.FoodCategory.IsAvailable && !f.FoodCategory.IsDeleted)
                .Select(f => new
                {
                    f.Id,
                    Avg = _context.FoodRatings.Where(r => r.FoodId == f.Id)
                                              .Select(r => (double?)r.Score).Average() ?? 0.0,
                    Voters = _context.FoodRatings.Count(r => r.FoodId == f.Id)
                })
                .OrderByDescending(x => x.Avg)
                .ThenByDescending(x => x.Voters)
                .Take(take)
                .Select(x => x.Id)
                .ToListAsync();

            return await _context.Foods
                .AsNoTracking()
                .Where(f => topIds.Contains(f.Id))
                .Include(f => f.Restaurant)
                .ToListAsync();
        }

        /* ===================== Restaurant Page ===================== */

        public async Task<List<Food>> GetRestaurantMenuBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return new List<Food>();

            return await _context.Foods
                .AsNoTracking()
                .Include(f => f.FoodCategory)
                .Include(f => f.Ratings)
                .Include(f => f.Restaurant)
                    .ThenInclude(r => r.RestaurantCategory)
                .Where(f =>
                    f.Restaurant.Slug == slug &&
                    f.Restaurant.IsActive && f.Restaurant.IsApproved &&
                    f.IsAvailable && !f.IsDeleted &&
                    f.FoodCategory.IsAvailable && !f.FoodCategory.IsDeleted)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }
        
        // CRUD
        public async Task<List<Food>> GetFoodsListForAdminAsync(int restaurantId)
        {
            return await _context.Foods
                .Where(u => u.IsDeleted == false && u.IsAvailable == true && u.RestaurantId == restaurantId)
                .Include(f => f.FoodCategory).ToListAsync();
        }
        public async Task<bool> AddFoodAsync(Food food)
        {
            try
            {
                await _context.Foods.AddAsync(food);
                await _context.SaveAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
        public async Task<Food> GetFoodDetailsAsync(int foodId)
        {
            var food = await _context.Foods
                .Include(f => f.Variants)
                    .ThenInclude(v => v.Addons)   // ⬅️ Addons از داخل Variant لود میشه
                .FirstOrDefaultAsync(f => f.Id == foodId);

            if (food is null)
                throw new Exception("food does not exist");

            return food;
        }
        public async Task<bool> DeleteFoodAsync(int foodId)
        {
            var food = await _context.Foods.FindAsync(foodId);
            if (food == null)
            {
                return false; // یعنی پیدا نشد
            }

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
