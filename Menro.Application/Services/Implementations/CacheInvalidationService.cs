using Menro.Application.Common.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Infrastructure.Services
{
    /// <summary>
    /// Centralized cache invalidation layer connecting repositories.
    /// This class ensures that whenever domain data changes (restaurants, orders, categories, etc.),
    /// related cached data in repositories is cleared safely and consistently.
    /// </summary>
    public class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IGlobalFoodCategoryRepository _globalCategoryRepository;

        public CacheInvalidationService(
            IRestaurantRepository restaurantRepository,
            IOrderRepository orderRepository,
            IGlobalFoodCategoryRepository globalCategoryRepository)
        {
            _restaurantRepository = restaurantRepository;
            _orderRepository = orderRepository;
            _globalCategoryRepository = globalCategoryRepository;
        }

        /* ============================================================
           🍽️ Restaurant-related
        ============================================================ */
        public void AllRestaurants()
        {
            _restaurantRepository.InvalidateFeaturedRestaurants();
            _restaurantRepository.InvalidateRandomRestaurants();
            _restaurantRepository.InvalidateBannerIds();
        }

        public void RestaurantBasic(string slug, string ownerUserId)
        {
            _restaurantRepository.InvalidateFeaturedRestaurants();
            _restaurantRepository.InvalidateRandomRestaurants();
            _restaurantRepository.InvalidateRestaurantBanner(slug);
            _restaurantRepository.InvalidateRestaurantIdByUser(ownerUserId);
        }

        public void Discounts()
        {
            _restaurantRepository.InvalidateRandomRestaurants();
        }

        public void AdBanners()
        {
            _restaurantRepository.InvalidateBannerIds();
        }

        /* ============================================================
           🛒 Orders / User Recent Orders
        ============================================================ */
        public void UserRecentOrders(string userId)
        {
            _orderRepository.InvalidateUserRecentOrders(userId);
        }

        /* ============================================================
           🌍 Global Categories + Popular Foods
        ============================================================ */
        public void GlobalCategories()
        {
            _globalCategoryRepository.InvalidateGlobalCategoryLists();
        }

        public void PopularFoodsByCategory(int categoryId)
        {
            _globalCategoryRepository.InvalidatePopularFoodsByCategory(categoryId);
        }
    }
}
