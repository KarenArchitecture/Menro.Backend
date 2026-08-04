using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Menro.Application.Features.Blog.Services.Implementations
{
    public class BlogRestaurantSearchService : IBlogRestaurantSearchService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public BlogRestaurantSearchService(
            IRestaurantRepository restaurantRepository,
            IMediaStorageProvider mediaStorage)
        {
            _restaurantRepository = restaurantRepository;
            _mediaStorage = mediaStorage;
        }

        public async Task<IReadOnlyList<BlogRestaurantSearchResult>> SearchAsync(
            string? term, int take = 10, CancellationToken ct = default)
        {
            take = Math.Clamp(take, 1, 30);
            var query = _restaurantRepository
                .QueryForAdmin(RestaurantStatus.Approved, term, categoryId: null)
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Ratings)
                .OrderBy(r => r.Name)
                .Take(take);

            var restaurants = await query.ToListAsync(ct);
            return restaurants.Select(MapToResult).ToList();
        }

        // NEW - refetches a single restaurant's current info (used by the
        // "refresh" button on an existing restaurant card in blog content).
        // Only returns Approved restaurants - same visibility rule as
        // SearchAsync, so a card can never refresh into something the
        // public site wouldn't actually show.
        public async Task<BlogRestaurantSearchResult?> GetByIdAsync(
            int id, CancellationToken ct = default)
        {
            var restaurant = await _restaurantRepository
                .QueryForAdmin(RestaurantStatus.Approved, search: null, categoryId: null)
                .Include(r => r.RestaurantCategory)
                .Include(r => r.Ratings)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            return restaurant is null ? null : MapToResult(restaurant);
        }

        private BlogRestaurantSearchResult MapToResult(Restaurant r)
        {
            return new BlogRestaurantSearchResult(
                r.Id,
                r.Name,
                r.RestaurantCategory.Name,
                string.IsNullOrWhiteSpace(r.LogoImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(
                        MediaCategory.RestaurantLogo,
                        r.LogoImageUrl,
                        r.Id.ToString(),
                        MediaVariant.Thumbnail),
                string.IsNullOrWhiteSpace(r.BannerImageUrl)
                    ? null
                    : _mediaStorage.GetUrl(
                        MediaCategory.RestaurantHomeBanner,
                        r.BannerImageUrl,
                        r.Id.ToString(),
                        MediaVariant.Resized),
                Math.Round(r.AverageRating, 1),
                r.VotersCount,
                r.Slug);
        }
    }
}