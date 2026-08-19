using Menro.Domain.Interfaces;
using Menro.Domain.Interfaces.Blog;
using Menro.Domain.Interfaces.Landing;
using Menro.Domain.Interfaces.Music;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Repositories.Blog;
using Menro.Infrastructure.Repositories.Landing;
using Menro.Infrastructure.Repositories.Music;
using Microsoft.Extensions.Caching.Memory;

namespace Menro.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly MenroDbContext _context;

        private IUserRepository _user;
        private IFoodRepository _food;
        private ICustomFoodCategoryRepository _foodCategory;
        private IRestaurantRepository _restaurant;
        private IRestaurantTableRepository _restaurantTable;
        private IRestaurantCategoryRepository _restaurantCategory;
        private IAdPricingSettingRepository _adPricingSetting;
        private IRestaurantAdRepository _restaurantAd;
        private ISubscriptionRepository _subscription;
        private ISubscriptionPlanRepository _subscriptionPlan;
        private IOtpRepository _otp;
        private ICartRepository _cart;
        private IOrderRepository _order;
        private IOrderItemRepository _orderItem;
        private IRefreshTokenRepository _refreshToken;
        private readonly IMemoryCache _cache;

        // MUSIC
        private IMusicTrackRepository _musicTrack;
        private IPlaylistRepository _playlist;
        private IPlaylistTrackRepository _playlistTrack;
        private IMusicPlayerRepository _musicPlayer;
        private ITrackRequestRepository _trackRequest;

        // BLOG
        private IBlogHeroRepository _blogHero;
        private IBlogPostRepository _blogPost;
        private IBlogPostContentRepository _blogPostContent;
        private IBlogCategoryRepository _blogCategory;
        private IBlogTagRepository _blogTag;
        private IBlogPostLikeRepository _blogPostLike;

        // LANDING
        private ILandingGeneralRepository _landingGeneral;
        private ILandingFaqRepository _landingFaq;
        private ILandingReasonRepository _landingReason;

        public IUserRepository User => _user ??= new UserRepository(_context);
        public IFoodRepository Food => _food ??= new FoodRepository(_context);
        public ICustomFoodCategoryRepository FoodCategory => _foodCategory ??= new CustomFoodCategoryRepository(_context, _cache);
        public IRestaurantRepository Restaurant => _restaurant ??= new RestaurantRepository(_context, _cache);
        public IRestaurantTableRepository RestaurantTable => _restaurantTable ??= new RestaurantTableRepository(_context);
        public IRestaurantCategoryRepository RestaurantCategory => _restaurantCategory ??= new RestaurantCategoryRepository(_context);
        public IAdPricingSettingRepository AdPricingSetting => _adPricingSetting ??= new AdPricingSettingRepository(_context);
        public IRestaurantAdRepository RestaurantAd => _restaurantAd ??= new RestaurantAdRepository(_context);
        public ISubscriptionRepository Subscription => _subscription ??= new SubscriptionRepository(_context);
        public ISubscriptionPlanRepository SubscriptionPlan => _subscriptionPlan ??= new SubscriptionPlanRepository(_context);
        public IOtpRepository Otp => _otp ??= new OtpRepository(_context);
        public ICartRepository Cart => _cart ??= new CartRepository(_context);
        public IOrderRepository Order => _order ??= new OrderRepository(_context, _cache);
        public IOrderItemRepository OrderItem => _orderItem ??= new OrderItemRepository(_context);
        public IRefreshTokenRepository RefreshToken => _refreshToken ??= new RefreshTokenRepository(_context);

        // MUSIC
        public IMusicTrackRepository MusicTrack => _musicTrack ??= new MusicTrackRepository(_context);
        public IPlaylistRepository Playlist => _playlist ??= new PlaylistRepository(_context);
        public IPlaylistTrackRepository PlaylistTrack => _playlistTrack ??= new PlaylistTrackRepository(_context);
        public IMusicPlayerRepository MusicPlayer => _musicPlayer ??= new MusicPlayerRepository(_context);
        public ITrackRequestRepository TrackRequest => _trackRequest ??= new TrackRequestRepository(_context);

        // BLOG
        public IBlogHeroRepository BlogHero => _blogHero ??= new BlogHeroRepository(_context);
        public IBlogPostRepository BlogPost => _blogPost ??= new BlogPostRepository(_context, _cache);
        public IBlogPostContentRepository BlogPostContent => _blogPostContent ??= new BlogPostContentRepository(_context);
        public IBlogCategoryRepository BlogCategory => _blogCategory ??= new BlogCategoryRepository(_context);
        public IBlogTagRepository BlogTag => _blogTag ??= new BlogTagRepository(_context);
        public IBlogPostLikeRepository BlogPostLike => _blogPostLike ??= new BlogPostLikeRepository(_context);

        // LANDING
        public ILandingGeneralRepository LandingGeneral => _landingGeneral ??= new LandingGeneralRepository(_context);
        public ILandingFaqRepository LandingFaq => _landingFaq ??= new LandingFaqRepository(_context);
        public ILandingReasonRepository LandingReason => _landingReason ??= new LandingReasonRepository(_context);

        public UnitOfWork(MenroDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing) _context.Dispose();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}