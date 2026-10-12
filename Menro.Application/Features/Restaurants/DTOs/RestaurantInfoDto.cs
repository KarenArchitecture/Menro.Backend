namespace Menro.Application.Features.Restaurants.DTOs
{
    /// <summary>
    /// DTO عمومی صفحه‌ی «اطلاعات رستوران».
    /// عمداً هیچ فیلد حساسی (کد ملی، شماره حساب، شبا، دلیل رد، صاحب رستوران) ندارد.
    /// </summary>
    public class RestaurantInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public string? LogoImageUrl { get; set; }
        public string? BannerImageUrl { get; set; }

        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;

        // فرمت "HH:mm"
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;

        public double AverageRating { get; set; }
        public int VotersCount { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // لینک‌های ساخته‌شده توسط سرور؛ اگه رستوران تنظیم نکرده باشه null
        public string? WhatsAppUrl { get; set; }
        public string? TelegramUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? EitaaUrl { get; set; }
        public string? BaleUrl { get; set; }
    }
}