using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Options;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RestaurantInfoService : IRestaurantInfoService
    {
        #region DI
        private readonly IUnitOfWork _uow;
        private readonly IMediaStorageProvider _mediaStorage;
        private readonly SocialLinksOptions _links;

        public RestaurantInfoService(
            IUnitOfWork uow,
            IMediaStorageProvider mediaStorage,
            IOptions<SocialLinksOptions> linksOptions)
        {
            _uow = uow;
            _mediaStorage = mediaStorage;
            _links = linksOptions.Value;
        }
        #endregion

        public async Task<RestaurantInfoDto?> GetInfoBySlugAsync(string slug, CancellationToken ct = default)
        {
            var r = await _uow.Restaurant.GetPublicInfoBySlugAsync(slug, ct);
            if (r == null) return null;

            var entityId = r.Id.ToString();

            return new RestaurantInfoDto
            {
                Name = r.Name,
                Slug = r.Slug,

                LogoImageUrl = string.IsNullOrWhiteSpace(r.LogoImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantLogo, r.LogoImageUrl, entityId, MediaVariant.Resized),

                BannerImageUrl = string.IsNullOrWhiteSpace(r.ShopBannerImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantShopBanner, r.ShopBannerImageUrl, entityId, MediaVariant.Resized),

                Address = r.Address,
                Description = r.Description,
                ContactNumber = r.ContactNumber,
                OpenTime = r.OpenTime.ToString(@"hh\:mm"),
                CloseTime = r.CloseTime.ToString(@"hh\:mm"),

                AverageRating = Math.Round(r.AverageRating, 1),
                VotersCount = r.VotersCount,

                Latitude = r.Latitude,
                Longitude = r.Longitude,

                WhatsAppUrl = BuildWhatsAppUrl(r.WhatsAppId),
                TelegramUrl = BuildUrl(_links.Telegram, r.TelegramId),
                InstagramUrl = BuildUrl(_links.Instagram, r.InstagramId),
                EitaaUrl = BuildUrl(_links.Eitaa, r.EitaaId),
                BaleUrl = BuildUrl(_links.Bale, r.BaleId)
            };
        }

        #region URL builders

        // ورودی قبلاً موقع ذخیره باید validate بشه؛ اینجا یه لایه‌ی دفاعی هم داریم
        // و فقط کاراکترهای مجاز رو قبول می‌کنیم.
        private string? BuildUrl(string baseUrl, string? id)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) return null;

            var clean = SanitizeId(id);
            return clean == null ? null : baseUrl + clean;
        }

        private string? BuildWhatsAppUrl(string? phone)
        {
            if (string.IsNullOrWhiteSpace(_links.WhatsApp) || string.IsNullOrWhiteSpace(phone))
                return null;

            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length == 0) return null;

            // 09123456789 → 989123456789 (فرمت بین‌المللی بدون +)
            if (digits.StartsWith("0")) digits = "98" + digits[1..];

            return _links.WhatsApp + digits;
        }

        private static string? SanitizeId(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var clean = id.Trim().TrimStart('@');
            var valid = clean.Length > 0
                        && clean.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');

            return valid ? clean : null;
        }

        #endregion
    }
}