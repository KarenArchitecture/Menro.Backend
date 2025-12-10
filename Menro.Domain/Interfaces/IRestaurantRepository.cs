using Menro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for managing Restaurant entities and related operations.
    /// Includes both query methods (for read operations) and cache invalidation helpers.
    /// </summary>
    public interface IRestaurantRepository : IRepository<Restaurant>
    {
        /* ============================================================
           🔹 Basic Lookups
        ============================================================ */

        Task<Restaurant?> GetByIdAsync(int id);

        /// <summary>
        /// Gets the restaurant name by its ID (with caching).
        /// </summary>
        Task<string> GetRestaurantName(int restaurantId);

        /* ============================================================
           🔹 Home Page - Featured Restaurants (Carousel)
        ============================================================ */

        /// <summary>
        /// Returns a list of restaurants marked as featured and approved.
        /// Used for the home page carousel section.
        /// </summary>
        Task<IEnumerable<Restaurant>> GetFeaturedRestaurantsAsync();

        /* ============================================================
           🔹 Home Page - Random Restaurant Cards
        ============================================================ */

        /// <summary>
        /// Returns a randomized list of active and approved restaurants,
        /// including details such as ratings, discounts, and categories.
        /// </summary>
        /// <param name="count">Number of restaurants to fetch.</param>
        Task<List<Restaurant>> GetRandomActiveApprovedWithDetailsAsync(int count);

        /* ============================================================
           🔹 Home Page - Advertisement Banners
        ============================================================ */

        /// <summary>
        /// Returns a random live advertisement banner, excluding already-served IDs.
        /// </summary>
        Task<RestaurantAdBanner?> GetRandomLiveAdBannerAsync(IEnumerable<int> excludeIds);

        /// <summary>
        /// Increments the impression counter atomically for a specific banner.
        /// Prevents overshoot beyond purchased views.
        /// </summary>
        Task<bool> IncrementBannerImpressionAsync(int bannerId);

        /* ============================================================
           🔹 Home Page - User’s Recent Orders
        ============================================================ */

        /// <summary>
        /// Returns restaurants ordered by a specific user, sorted by latest order time.
        /// </summary>
        Task<List<Restaurant>> GetRestaurantsOrderedByUserAsync(string userId);

        /* ============================================================
           🔹 Restaurant Page (Banner + Slug + Validation)
        ============================================================ */

        /// <summary>
        /// Retrieves a restaurant’s banner and basic details by slug.
        /// </summary>
        Task<Restaurant?> GetRestaurantBannerBySlugAsync(string slug);

        /// <summary>
        /// Checks if a restaurant slug already exists.
        /// </summary>
        Task<bool> SlugExistsAsync(string slug);

        /// <summary>
        /// Returns the restaurant ID for a specific owner (userId).
        /// </summary>
        Task<int> GetRestaurantIdByUserIdAsync(string userId);

        /* ============================================================
           🔄 Cache Invalidation Helpers
        ============================================================ */

        /// <summary>
        /// Removes cached featured restaurants.
        /// </summary>
        void InvalidateFeaturedRestaurants();

        /// <summary>
        /// Removes cached random restaurant list.
        /// </summary>
        void InvalidateRandomRestaurants();

        /// <summary>
        /// Removes cached restaurant banner data for a specific slug.
        /// </summary>
        void InvalidateRestaurantBanner(string slug);

        /// <summary>
        /// Removes cached restaurant ID lookup by user.
        /// </summary>
        void InvalidateRestaurantIdByUser(string userId);

        /// <summary>
        /// Removes cached live advertisement banner IDs.
        /// </summary>
        void InvalidateBannerIds();

        // CRUD
        Task SaveChangesAsync();


        // admin panel => restaurant management tab
        Task<List<Restaurant>> GetRestaurantsListForAdminAsync(bool? approvedStatus = null);
        Task<Restaurant?> GetRestaurantDetailsForAdminAsync(int id);

        // restaurant profile
        Task<Restaurant?> GetRestaurantProfileAsync(int restaurantId);
    }
}
