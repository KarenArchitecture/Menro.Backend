using Menro.Domain.Interfaces.Blog;
using Menro.Domain.Interfaces.Landing;
using Menro.Domain.Interfaces.Music;

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
        
        // MUSIC
        IMusicTrackRepository MusicTrack { get;}
        IPlaylistRepository Playlist { get;}
        IPlaylistTrackRepository PlaylistTrack { get;}
        IMusicPlayerRepository MusicPlayer { get;}
        ITrackRequestRepository TrackRequest { get;}

        // BLOG
        IBlogHeroRepository BlogHero { get;}
        IBlogPostRepository BlogPost { get;}
        IBlogCategoryRepository BlogCategory { get;}
        IBlogTagRepository BlogTag { get;}

        // LANDING
        ILandingGeneralRepository LandingGeneral { get;}
        ILandingFaqRepository LandingFaq { get;}
        ILandingReasonRepository LandingReason { get;}

        Task<int> SaveChangesAsync();
    }
}
