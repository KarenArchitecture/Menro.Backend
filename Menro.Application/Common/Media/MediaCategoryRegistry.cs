namespace Menro.Application.Common.Media
{
    public static class MediaCategoryRegistry
    {
        public static readonly IReadOnlyDictionary<MediaCategory, MediaCategoryConfig> All =
            new Dictionary<MediaCategory, MediaCategoryConfig>
            {
                // global / not tied to a specific entity -> flat storage, no variants (vector file)
                [MediaCategory.FoodCategoryIcon] = new()
                {
                    FolderTemplate = "media/icons",
                    AllowedExtensions = new[] { ".svg" },
                    MaxSizeBytes = 100 * 1024,
                    IsEntityScoped = false,
                    PreserveOriginalFileName = true,
                },

                // entity-scoped by userId
                [MediaCategory.UserProfileImage] = new()
                {
                    FolderTemplate = "media/img/profile",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 5 * 1024 * 1024,
                    IsEntityScoped = true,
                    ThumbnailWidth = 150,
                    ResizedWidth = 600,
                },

                // entity-scoped by restaurantId
                [MediaCategory.RestaurantLogo] = new()
                {
                    FolderTemplate = "media/img/restaurant/logo",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 5 * 1024 * 1024,
                    IsEntityScoped = true,
                    ThumbnailWidth = 150,
                    ResizedWidth = 500,
                },
                [MediaCategory.RestaurantHomeBanner] = new()
                {
                    FolderTemplate = "media/img/restaurant/home",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 8 * 1024 * 1024,
                    IsEntityScoped = true,
                    ResizedWidth = 1600,
                },
                [MediaCategory.RestaurantShopBanner] = new()
                {
                    FolderTemplate = "media/img/restaurant/shop",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 8 * 1024 * 1024,
                    IsEntityScoped = true,
                    ResizedWidth = 1600,
                },

                // entity-scoped by foodId
                [MediaCategory.RestaurantFoodImage] = new()
                {
                    FolderTemplate = "media/img/food",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 4 * 1024 * 1024,
                    IsEntityScoped = true,
                    ThumbnailWidth = 200,   // menu list / grid
                    ResizedWidth = 800,     // food detail view
                },

                // entity-scoped by restaurantId
                [MediaCategory.RestaurantAdBanner] = new()
                {
                    FolderTemplate = "media/img/ads/banner",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 4 * 1024 * 1024,
                    IsEntityScoped = true,
                    ResizedWidth = 1200,
                },
                [MediaCategory.RestaurantAdCarousel] = new()
                {
                    FolderTemplate = "media/img/ads/carousel",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 4 * 1024 * 1024,
                    IsEntityScoped = true,
                    ThumbnailWidth = 300,
                    ResizedWidth = 1200,
                },

                // entity-scoped by restaurantId, not an image -> no variants
                [MediaCategory.RestaurantMusicFile] = new()
                {
                    FolderTemplate = "media/music/files",
                    AllowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".ogg" },
                    MaxSizeBytes = 25 * 1024 * 1024,
                    IsEntityScoped = true,
                    IsPublic = false,
                },
                [MediaCategory.RestaurantMusicCover] = new()
                {
                    FolderTemplate = "media/music/covers",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 2 * 1024 * 1024,
                    IsEntityScoped = true,
                    ThumbnailWidth = 150,
                    // small cover art, no need for an extra "resized" tier
                },

                // entity-scoped by blogPostId
                [MediaCategory.BlogPostImage] = new()
                {
                    FolderTemplate = "media/img/blog/posts",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 6 * 1024 * 1024,
                    IsEntityScoped = true,
                    ThumbnailWidth = 300,
                    ResizedWidth = 1200,
                },
                [MediaCategory.BlogContentImage] = new()
                {
                    FolderTemplate = "media/img/blog/content",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 6 * 1024 * 1024,
                    IsEntityScoped = true,
                    ResizedWidth = 1600,
                },
                // global / site-wide -> flat storage
                [MediaCategory.LandingHeroImage] = new()
                {
                    FolderTemplate = "media/img/landing/hero",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 8 * 1024 * 1024,
                    IsEntityScoped = false,
                    ResizedWidth = 1920,
                },
            };
    }
}
