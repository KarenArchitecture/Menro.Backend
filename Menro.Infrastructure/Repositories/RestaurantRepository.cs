using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantRepository : Repository<Restaurant>, IRestaurantRepository
    {
        private readonly MenroDbContext _context;

        public RestaurantRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        //Home Page - Featured Restaurants Carousel
        public async Task<IEnumerable<Restaurant>> GetFeaturedRestaurantsAsync()
        {
            return await _context.Restaurants
                .Where(r => r.IsFeatured)
                .ToListAsync();
        }

        //Home Page - Random Restaurants Cards
        public async Task<List<Restaurant>> GetAllActiveApprovedWithDetailsAsync()
        {
            return await _context.Restaurants
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Ratings)
                .Include(r => r.Discounts)
                .Where(r => r.IsActive && r.IsApproved)
                .ToListAsync();
        }

        //Home Page - Featured Restaurant Banner
        public async Task<RestaurantAdBanner?> GetActiveAdBannerAsync()
        {
            return await _context.RestaurantAdBanners
                .Include(b => b.Restaurant)
                .FirstOrDefaultAsync(b => b.StartDate <= DateTime.UtcNow && b.EndDate >= DateTime.UtcNow);
        }

        //Home Page - Latest Orders
        public async Task<List<Restaurant>> GetRestaurantsOrderedByUserAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => o.Restaurant)
                .Distinct()
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Ratings)
                .Include(r => r.Discounts)
                .ToListAsync();
        }

        //Shop Page - Restaurant Banner 
        public async Task<Restaurant?> GetBySlugWithCategoryAsync(string slug)
        {
            return await _context.Restaurants
                .Include(r => r.RestaurantCategory)
                .FirstOrDefaultAsync(r => r.Slug == slug && r.IsActive && r.IsApproved);
        }

        //Shop Page - Preventing Save For an Existing Slug
        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Restaurants.AnyAsync(r => r.Slug == slug);
        }
        public async Task<int?> GetRestaurantIdByUserIdAsync(string userId)
        {
            return await _context.Restaurants
                .Where(r => r.OwnerUserId == userId)
                .Select(r => (int?)r.Id)
                .FirstOrDefaultAsync();
        }

    }
}
