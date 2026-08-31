using Menro.Domain.Interfaces.Blog;
using Menro.Domain.Interfaces.Music;
using Menro.Domain.Interfaces.SiteContent;

namespace Menro.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; }
        IFoodRepository Food { get; }
        ICustomFoodCategoryRepository FoodCategory { get; }
        IRestaurantRepository Restaurant { get; }
        IRestaurantTableRepository RestaurantTable { get; }
        IRestaurantCategoryRepository RestaurantCategory { get; }
        IAdPricingSettingRepository AdPricingSetting { get; }
        IRestaurantAdRepository RestaurantAd { get; }
        ISubscriptionRepository Subscription { get; }
        ISubscriptionPlanRepository SubscriptionPlan { get; }
        IOtpRepository Otp { get; }
        ICartRepository Cart { get; }
        IOrderRepository Order { get; }
        IOrderItemRepository OrderItem { get; }
        IRefreshTokenRepository RefreshToken { get; }

        // MUSIC
        IMusicTrackRepository MusicTrack { get; }
        IPlaylistRepository Playlist { get; }
        IPlaylistTrackRepository PlaylistTrack { get; }
        IMusicPlayerRepository MusicPlayer { get; }
        ITrackRequestRepository TrackRequest { get; }

        // BLOG
        IBlogHeroRepository BlogHero { get; }
        IBlogPostRepository BlogPost { get; }
        IBlogPostContentRepository BlogPostContent { get; }
        IBlogCategoryRepository BlogCategory { get; }
        IBlogTagRepository BlogTag { get; }
        IBlogPostLikeRepository BlogPostLike { get; }

        // SITE CONTENT
        ILandingGeneralRepository LandingGeneral { get; }
        ILandingFaqRepository LandingFaq { get; }
        ILandingReasonRepository LandingReason { get; }
        IMenuItemRepository MenuItem { get; }

        Task<int> SaveChangesAsync();
    }
}