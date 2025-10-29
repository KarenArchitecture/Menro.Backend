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
            return await _context.GlobalFoodCategories.Include(c => c.Icon).OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<GlobalFoodCategory> GetByIdAsync(int Id)
        {
            var cat = await _context.GlobalFoodCategories.Include(g => g.Icon).FirstOrDefaultAsync(g => g.Id == Id);

            if (cat is null)
            {
                throw new Exception("food category does not exist");
            }

            return cat;
        }
     


    }
}
