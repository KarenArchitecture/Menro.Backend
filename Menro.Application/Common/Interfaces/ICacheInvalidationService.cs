using System.Threading.Tasks;

namespace Menro.Application.Common.Interfaces
{
    /// <summary>
    /// Centralized service for triggering cache invalidation across repositories.
    /// Keeps cache reset logic consistent and reusable from other layers (services, handlers, controllers).
    /// </summary>
    public interface ICacheInvalidationService
    {
        /* ============================================================
           🍽️ Restaurant-related
        ============================================================ */
        void AllRestaurants();                        // Full reset for any restaurant change
        void RestaurantBasic(string slug, string ownerUserId);
        void Discounts();
        void AdBanners();
        void RestaurantCategories(string slug);       // ✅ NEW: invalidate custom/global categories of a restaurant

        /* ============================================================
           🛒 Orders / User Recent Orders
        ============================================================ */
        void UserRecentOrders(string userId);

        /* ============================================================
           🌍 Global Categories + Popular Foods
        ============================================================ */
        void GlobalCategories();                      // Invalidate cached eligible global categories
        void PopularFoodsByCategory(int categoryId);  // Invalidate cached popular foods for one category
    }
}
