using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RestaurantRatingService : IRestaurantRatingService
    {
        private readonly IRestaurantRatingRepository _repo;
        public RestaurantRatingService(IRestaurantRatingRepository repo) => _repo = repo;

        public async Task<RestaurantRatingResultDto> SubmitRatingAsync(string userId, RateRestaurantRequestDto dto, CancellationToken ct = default)
        {
            if (dto.Score < 1 || dto.Score > 5)
                throw new Exception("امتیاز باید بین ۱ تا ۵ باشد.");

            var canRate = await _repo.UserCanRateRestaurantAsync(userId, dto.RestaurantId, ct);
            if (!canRate)
                throw new Exception("برای امتیازدهی باید حداقل یک سفارش از این رستوران ثبت کرده باشید.");

            var existing = await _repo.GetByUserAndRestaurantAsync(userId, dto.RestaurantId, ct);
            if (existing != null)
            {
                existing.Score = dto.Score;
            }
            else
            {
                await _repo.AddAsync(new RestaurantRating
                {
                    RestaurantId = dto.RestaurantId,
                    UserId = userId,
                    Score = dto.Score,
                    CreatedAt = DateTime.UtcNow
                }, ct);
            }

            await _repo.SaveChangesAsync(ct);

            var (avg, voters) = await _repo.GetAggregateAsync(dto.RestaurantId, ct);
            return new RestaurantRatingResultDto
            {
                AverageRating = avg,
                VotersCount = voters,
                MyScore = dto.Score
            };
        }

        public async Task<RestaurantRatingResultDto> GetRatingSummaryAsync(string? userId, int restaurantId, CancellationToken ct = default)
        {
            var (avg, voters) = await _repo.GetAggregateAsync(restaurantId, ct);

            int? myScore = null;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var mine = await _repo.GetByUserAndRestaurantAsync(userId, restaurantId, ct);
                myScore = mine?.Score;
            }

            return new RestaurantRatingResultDto
            {
                AverageRating = avg,
                VotersCount = voters,
                MyScore = myScore
            };
        }
    }
}