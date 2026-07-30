// Application/FoodRatings/Services/Implementations/UpsertFoodRatingService.cs
using Menro.Application.FoodRatings.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.FoodRatings.Services.Implementations
{
    public class UpsertFoodRatingService : IUpsertFoodRatingService
    {
        private readonly IFoodRatingRepository _foodRatingRepository;

        public UpsertFoodRatingService(IFoodRatingRepository foodRatingRepository)
        {
            _foodRatingRepository = foodRatingRepository;
        }

        public async Task UpsertAsync(string userId, int foodId, int score)
        {
            if (score < 1 || score > 5)
                throw new ArgumentOutOfRangeException(nameof(score), "امتیاز باید بین ۱ تا ۵ باشد.");

            var existing = await _foodRatingRepository.GetByFoodAndUserAsync(foodId, userId);

            if (existing != null)
            {
                existing.Score = score;
                existing.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                await _foodRatingRepository.AddAsync(new FoodRating
                {
                    FoodId = foodId,
                    UserId = userId,
                    Score = score,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _foodRatingRepository.SaveChangesAsync();
        }
    }
}