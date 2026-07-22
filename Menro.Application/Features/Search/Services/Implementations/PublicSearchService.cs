using Menro.Application.Features.Search.DTOs;
using Menro.Application.Features.Search.Services.Interfaces;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Search.Services.Implementations
{
    public class PublicSearchService : IPublicSearchService
    {
        private readonly ISearchRepository _repo;

        public PublicSearchService(ISearchRepository repo)
        {
            _repo = repo;
        }

        private static string? ToHm(TimeSpan? t) => t.HasValue ? t.Value.ToString(@"hh\:mm") : null;

        public async Task<SearchResponseDto> SearchAsync(string term, int take = 15)
        {
            take = Math.Clamp(take, 1, 50);

            var hits = await _repo.SearchAsync(term, take);

            var items = hits.Select(h => new SearchItemDto
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

                // Restaurant extras (safe for Food too)
                LogoImageUrl = h.LogoImageUrl,
                Category = h.Category,
                OpenTime = ToHm(h.OpenTime),
                CloseTime = ToHm(h.CloseTime),
                Discount = h.Discount,
                Rating = h.Rating,
                Voters = h.Voters,
                IsOpen = h.IsOpen
            }).ToList();

            return new SearchResponseDto
            {
                Term = (term ?? "").Trim(),
                Items = items
            };
        }
    }
}
