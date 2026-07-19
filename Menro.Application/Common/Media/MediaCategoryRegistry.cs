namespace Menro.Application.Common.Media
{
    public static class MediaCategoryRegistry
    {
        public static readonly IReadOnlyDictionary<MediaCategory, MediaCategoryConfig> All =
            new Dictionary<MediaCategory, MediaCategoryConfig>
            {
                [MediaCategory.FoodCategoryIcon] = new()
                {
                    FolderTemplate = "icons",
                    AllowedExtensions = new[] { ".svg" },
                    MaxSizeBytes = 100 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.UserProfileImage] = new()
                {
                    FolderTemplate = "img/users/{ownerId}/profile",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 5 * 1024 * 1024,
                    IsEntityScoped = true,
                },
                [MediaCategory.RestaurantLogo] = new()
                {
                    FolderTemplate = "img/restaurants/{ownerId}/logo",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 5 * 1024 * 1024,
                    IsEntityScoped = true,
                },
                [MediaCategory.RestaurantHomeBanner] = new()
                {
                    FolderTemplate = "img/restaurants/{ownerId}/banners/home",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 8 * 1024 * 1024,
                    IsEntityScoped = true,
                },
                [MediaCategory.RestaurantShopBanner] = new()
                {
                    FolderTemplate = "img/restaurants/{ownerId}/banners/shop",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 8 * 1024 * 1024,
                    IsEntityScoped = true,
                },
                [MediaCategory.RestaurantFoodImage] = new()
                {
                    FolderTemplate = "img/restaurants/{ownerId}/food",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 4 * 1024 * 1024,
                    IsEntityScoped = true,
                },
                [MediaCategory.RestaurantAdBanner] = new()
                {
                    FolderTemplate = "img/ads/banner",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 4 * 1024 * 1024,
                    IsEntityScoped = false, // تبلیغات مرکزی، توسط ادمین مدیریت می‌شه
                },
                [MediaCategory.RestaurantAdCarousel] = new()
                {
                    FolderTemplate = "img/ads/carousel",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 4 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.RestaurantMusicFile] = new()
                {
                    FolderTemplate = "media/restaurants/{ownerId}/music/files",
                    AllowedExtensions = new[] { ".mp3", ".wav", ".m4a" },
                    MaxSizeBytes = 25 * 1024 * 1024,
                    IsEntityScoped = true,
                    IsPublic = false, // نه مستقیم public، از پشت یک اکشن کنترلر با auth سرو بشه
                },
                [MediaCategory.RestaurantMusicCover] = new()
                {
                    FolderTemplate = "media/restaurants/{ownerId}/music/covers",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 2 * 1024 * 1024,
                    IsEntityScoped = true,
                },
                [MediaCategory.BlogPostImage] = new()
                {
                    FolderTemplate = "img/blog/posts",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 6 * 1024 * 1024,
                    IsEntityScoped = false,
                },
                [MediaCategory.LandingHeroImage] = new()
                {
                    FolderTemplate = "img/landing/hero",
                    AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" },
                    MaxSizeBytes = 8 * 1024 * 1024,
                    IsEntityScoped = false,
                },
            };
    }
}
