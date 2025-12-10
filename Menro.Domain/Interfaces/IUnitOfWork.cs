namespace Menro.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; }
        IFoodRepository Food { get;}
        ICustomFoodCategoryRepository FoodCategory { get;}
        IRestaurantRepository Restaurant { get;}
        IRestaurantCategoryRepository RestaurantCategory { get;}
        IAdPricingSettingRepository AdPricingSetting { get;}
        IRestaurantAdRepository RestaurantAd { get;}
        ISubscriptionRepository Subscription { get;}
        ISubscriptionPlanRepository SubscriptionPlan { get;}
        IOtpRepository Otp { get;}
        IOrderRepository Order { get;}
        IOrderItemRepository OrderItem { get;}
        IRefreshTokenRepository RefreshToken { get;}
        Task<int> SaveChangesAsync();
    }
}
