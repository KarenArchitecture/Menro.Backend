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

        public async Task<bool> CreateAsync(CustomFoodCategory category)
        {
            try
            {
                await _context.CustomFoodCategories.AddAsync(category);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<IEnumerable<CustomFoodCategory>> GetAllAsync(int restaurantId)
        {
            return await _context.CustomFoodCategories.Include(u => u.Icon)
                .Where(u => u.RestaurantId == restaurantId && !u.IsDeleted && u.IsAvailable)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }
        public async Task<CustomFoodCategory> GetByIdAsync(int catId)
        {
            var category = await _context.CustomFoodCategories.Include(c => c.Icon).FirstOrDefaultAsync(c => c.Id == catId);
            
            if (category == null) throw new Exception($"Custom category with ID {catId} not found.");
            
            return category;
        }
        public async Task<CustomFoodCategory?> GetByNameAsync(int restaurantId, string catName)
        {
            return await _context.CustomFoodCategories
                .FirstOrDefaultAsync(c => c.RestaurantId == restaurantId && c.Name == catName);
        }
        public async Task<IEnumerable<CustomFoodCategory>> GetByRestaurantSlugAsync(string restaurantSlug)
        {
            return await _context.CustomFoodCategories
                .Where(fc => fc.Restaurant.Slug == restaurantSlug)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(int restaurantId, string catName)
        {
            return await _context.CustomFoodCategories.AnyAsync(u => u.RestaurantId == restaurantId && u.Name == catName);
        }
        public async Task<bool> IsSoftDeleted(int restaurantId, string catName)
        {
            var cat = await _context.CustomFoodCategories.Where(c => c.IsDeleted == true && c.RestaurantId == restaurantId)
                .FirstOrDefaultAsync(x => x.Name == catName);
            return cat != null;
        }
        public async Task<bool> DeleteAsync(int catId)
        {
            var cat = await _context.CustomFoodCategories.Include(c => c.Foods).FirstOrDefaultAsync(c => c.Id == catId);
            if (cat is null)
            {
                return false;
            }
            if (cat.Foods.Count == 0)
            {
                _context.CustomFoodCategories.Remove(cat);
            }
            else
            {
                cat.IsDeleted = true;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateCategoryAsync(CustomFoodCategory category)
        {
            if (category == null) return false;

            _context.CustomFoodCategories.Update(category);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

    }
}
