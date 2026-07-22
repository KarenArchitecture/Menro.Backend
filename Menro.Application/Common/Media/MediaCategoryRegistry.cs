namespace Menro.Application.Common.Media
{
    public static class MediaCategoryRegistry
    {
        public static readonly IReadOnlyDictionary<MediaCategory, MediaCategoryConfig> All =
            new Dictionary<MediaCategory, MediaCategoryConfig>
            {
                [MediaCategory.FoodCategoryIcon] = new()
                {
                    FolderTemplate = "media/icons",
                    AllowedExtensions = new[] { ".svg" },
                    MaxSizeBytes = 100 * 1024,
                    IsEntityScoped = false,
                    PreserveOriginalFileName = true,
                },
                [MediaCategory.UserProfileImage] = new()
                {
                    FolderTemplate = "media/img/profile",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 5 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.RestaurantLogo] = new()
                {
                    FolderTemplate = "media/img/restaurant/logo",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 5 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.RestaurantCard] = new()
                {
                    FolderTemplate = "media/img/restaurant/card",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 4 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.RestaurantHomeBanner] = new()
                {
                    FolderTemplate = "media/img/restaurant/home",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 8 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.RestaurantShopBanner] = new()
                {
                    FolderTemplate = "media/img/restaurant/shop",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 8 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.RestaurantFoodImage] = new()
                {
                    FolderTemplate = "media/img/food",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 4 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.RestaurantAdBanner] = new()
                {
                    FolderTemplate = "media/img/ads/banner",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 4 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.RestaurantAdCarousel] = new()
                {
                    FolderTemplate = "media/img/ads/carousel",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 4 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.RestaurantMusicFile] = new()
                {
                    FolderTemplate = "media/music/files",
                    AllowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".ogg" },
                    MaxSizeBytes = 25 * 1024 * 1024,
                    IsEntityScoped = false,
                    IsPublic = false,
                },
                [MediaCategory.RestaurantMusicCover] = new()
                {
                    FolderTemplate = "media/music/covers",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 2 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.BlogPostImage] = new()
                {
                    FolderTemplate = "media/img/blog/posts",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 6 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.LandingHeroImage] = new()
                {
                    FolderTemplate = "media/img/landing/hero",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 8 * 1024 * 1024,
                    IsEntityScoped = false,
                },
            };
    }
}