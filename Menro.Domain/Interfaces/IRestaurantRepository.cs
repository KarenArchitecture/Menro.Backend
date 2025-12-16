using Menro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    public interface IRestaurantRepository : IRepository<Restaurant>
    {
        /* ============================================================
           🔹 Basic Lookups
        ============================================================ */
        Task<Restaurant?> GetByIdAsync(int id);

        Task<string> GetRestaurantName(int restaurantId);

        /* ============================================================
           🔹 Home Page - Featured Restaurants (Carousel)
        ============================================================ */


        /* ============================================================
           🔹 Home Page - Random Restaurant Cards
        ============================================================ */
        Task<List<Restaurant>> GetRandomActiveApprovedWithDetailsAsync(int count);

        /* ============================================================
           🔹 Home Page - Advertisement Banners
        ============================================================ */
        Task<bool> IncrementBannerImpressionAsync(int bannerId);

        /* ============================================================
           🔹 Home Page - User’s Recent Orders
        ============================================================ */
        Task<List<Restaurant>> GetRestaurantsOrderedByUserAsync(string userId);

        /* ============================================================
           🔹 Restaurant Page (Banner + Slug + Validation)
        ============================================================ */
        Task<Restaurant?> GetRestaurantBannerBySlugAsync(string slug);

        Task<bool> SlugExistsAsync(string slug);

        Task<int> GetRestaurantIdByUserIdAsync(string userId);

        /* ============================================================
           🔄 Cache Invalidation Helpers
        ============================================================ */
        void InvalidateFeaturedRestaurants();

        void InvalidateRandomRestaurants();

        void InvalidateRestaurantBanner(string slug);

        void InvalidateRestaurantIdByUser(string userId);

        void InvalidateBannerIds();

        // CRUD
        Task SaveChangesAsync();


        // admin panel => restaurant management tab
        Task<List<Restaurant>> GetRestaurantsListForAdminAsync();
        Task<Restaurant?> GetRestaurantDetailsForAdminAsync(int id);

        // restaurant profile
        Task<Restaurant?> GetRestaurantProfileAsync(int restaurantId);
    }
}
