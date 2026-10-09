using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Search.DTOs;
using Menro.Application.Features.Search.Services.Interfaces;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Search.Services.Implementations
{
    public class PublicSearchService : IPublicSearchService
    {
        private readonly ISearchRepository _repo;
        private readonly IMediaStorageProvider _mediaStorage;

        public PublicSearchService(ISearchRepository repo, IMediaStorageProvider mediaStorage)
        {
            _repo = repo;
            _mediaStorage = mediaStorage;
        }

        private static string? ToHm(TimeSpan? t) => t.HasValue ? t.Value.ToString(@"hh\:mm") : null;

        public async Task<SearchResponseDto> SearchAsync(string term, int take = 15)
        {
            take = Math.Clamp(take, 1, 50);

            var hits = await _repo.SearchAsync(term, take);

            var items = hits.Select(h =>
            {
                var imageUrl = h.ImageFileName;
                var logoUrl = h.LogoImageUrl;

                // Restaurants: turn stored file names into real URLs,
                // exactly like RestaurantBrowseService does for the normal list
                if (h.Type == SearchHitType.Restaurant)
                {
                    var entityId = h.Id.ToString();

                    imageUrl = string.IsNullOrWhiteSpace(h.ImageFileName)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantHomeBanner, h.ImageFileName, entityId, MediaVariant.Resized);

                    logoUrl = string.IsNullOrWhiteSpace(h.LogoImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantLogo, h.LogoImageUrl, entityId, MediaVariant.Thumbnail);
                }
                else
                {
                    // Foods: turn a bare file name into a real URL.
                    // If it's already a URL/path, leave it untouched.
                    var fileName = h.ImageFileName;
                    if (!string.IsNullOrWhiteSpace(fileName) &&
                        !fileName.StartsWith("/") &&
                        !fileName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        imageUrl = _mediaStorage.GetUrl(
                            MediaCategory.RestaurantFoodImage,
                            fileName,
                            h.Id.ToString(),
                            MediaVariant.Thumbnail);
                    }
                }

                return new SearchItemDto
                {
                    Type = h.Type == SearchHitType.Restaurant ? SearchItemType.Restaurant : SearchItemType.Food,
                    Id = h.Id,
                    Title = h.Title,
                    Subtitle = h.Subtitle,

                    ImageUrl = imageUrl,

                    RestaurantId = h.RestaurantId,
                    RestaurantSlug = h.RestaurantSlug,

                    TargetUrl = !string.IsNullOrWhiteSpace(h.RestaurantSlug)
                        ? $"/restaurant/{h.RestaurantSlug}"
                        : "",

                    // Restaurant extras (safe for Food too)
                    LogoImageUrl = logoUrl,
                    Category = h.Category,
                    OpenTime = ToHm(h.OpenTime),
                    CloseTime = ToHm(h.CloseTime),
                    Discount = h.Discount,
                    Rating = h.Rating,
                    Voters = h.Voters,
                    IsOpen = h.IsOpen
                };
            }).ToList();

            return new SearchResponseDto
            {
                Term = (term ?? "").Trim(),
                Items = items
            };
        }

        private static SearchItemDto ToDto(Menro.Domain.Contracts.SearchHit h) => new SearchItemDto
        {
            Type = h.Type == SearchHitType.Restaurant ? SearchItemType.Restaurant : SearchItemType.Food,
            Id = h.Id,
            Title = h.Title,
            Subtitle = h.Subtitle,
            ImageUrl = h.ImageFileName,
            RestaurantId = h.RestaurantId,
            RestaurantSlug = h.RestaurantSlug,
            TargetUrl = !string.IsNullOrWhiteSpace(h.RestaurantSlug)
        ? $"/restaurant/{h.RestaurantSlug}"
        : "",
            LogoImageUrl = h.LogoImageUrl,
            Category = h.Category,
            OpenTime = ToHm(h.OpenTime),
            CloseTime = ToHm(h.CloseTime),
            Discount = h.Discount,
            Rating = h.Rating,
            Voters = h.Voters,
            IsOpen = h.IsOpen
        };

        public async Task<PagedResultDto<SearchItemDto>> SearchPagedAsync(
            string term, SearchItemType type, int skip = 0, int take = 12, int? categoryId = null)
        {
            take = Math.Clamp(take, 1, 50);
            skip = Math.Max(skip, 0);

            var hitType = type == SearchItemType.Restaurant
                ? SearchHitType.Restaurant
                : SearchHitType.Food;

            var (hits, hasMore) = await _repo.SearchPagedAsync(term, hitType, skip, take, categoryId);

            return new PagedResultDto<SearchItemDto>
            {
                Items = hits.Select(ToDto).ToList(),
                HasMore = hasMore,
                NextCursor = hasMore ? (skip + take).ToString() : null
            };
        }
    }
}