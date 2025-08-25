using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    public interface IRestaurantRepository : IRepository<Restaurant>
    {
        Task<string> GetRestaurantName(int restaurantId);
        //Home Page - Featured Restaurants Carousel
        Task<IEnumerable<Restaurant>> GetFeaturedRestaurantsAsync();

        //Home Page - Random Restaurants Cards
        Task<List<Restaurant>> GetAllActiveApprovedWithDetailsAsync();

        //Home Page - Featured Restaurant Banner
        Task<RestaurantAdBanner> GetActiveAdBannerAsync();

        //Home Page - Latest Orders
        Task<List<Restaurant>> GetRestaurantsOrderedByUserAsync(string userId);

        //Shop Page - Restaurant Banner 
        Task<Restaurant?> GetBySlugWithCategoryAsync(string slug);

        //Shop Page - Preventing Save For an Existing Slug
        Task<bool> SlugExistsAsync(string slug);

        Task<int?> GetRestaurantIdByUserIdAsync(string userId);
    }
}
