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
        // Home Page - Random eligible Ad Banner (exclude already-served ids)
        Task<RestaurantAdBanner?> GetRandomLiveAdBannerAsync(IEnumerable<int> excludeIds);
        // Home Page - Count an impression atomically (no overshoot)
        Task<bool> IncrementBannerImpressionAsync(int bannerId);

        //Home Page - Latest Orders
        Task<List<Restaurant>> GetRestaurantsOrderedByUserAsync(string userId);

        //Restaurant Page - Restaurant Banner 
        Task<Restaurant?> GetBySlugWithRatingsAsync(string slug);

        //Restaurant Page - Preventing Save For an Existing Slug
        Task<bool> SlugExistsAsync(string slug);

        Task<int?> GetRestaurantIdByUserIdAsync(string userId);
    }
}
