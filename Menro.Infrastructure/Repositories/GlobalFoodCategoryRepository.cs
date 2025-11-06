using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class GlobalFoodCategoryRepository : IGlobalFoodCategoryRepository
    {
        private readonly MenroDbContext _context;
        public GlobalFoodCategoryRepository(MenroDbContext context)
        {
            _context = context;
        }
        public async Task<bool> CreateAsync(GlobalFoodCategory category)
        {
            try
            {
                _context.GlobalFoodCategories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<List<GlobalFoodCategory>> GetAllAsync()
        {
            return await _context.GlobalFoodCategories.Include(c => c.Icon).Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToListAsync();
        }
        public async Task<GlobalFoodCategory> GetByIdAsync(int id)
        {
            var cat = await _context.GlobalFoodCategories.Include(g => g.Icon).FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

            if (cat is null)
            {
                throw new Exception("food category does not exist");
            }

            return cat;
        }
        public async Task<bool> UpdateCategoryAsync(GlobalFoodCategory category)
        {
            if (category == null) return false;

            _context.GlobalFoodCategories.Update(category);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var cat = await _context.GlobalFoodCategories.Include(c => c.Foods).FirstOrDefaultAsync(c => c.Id == id);
            if (cat is null)
            {
                return false;
            }
            if (cat.Foods.Count == 0)
            {
                _context.GlobalFoodCategories.Remove(cat);
            }
            else
            {
                cat.IsDeleted = true;
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
