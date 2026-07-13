// Application/FoodRatings/Services/Interfaces/IUpsertFoodRatingService.cs
using Menro.Application.Features.FoodRatings.DTOs;

namespace Menro.Application.FoodRatings.Services.Interfaces
{
    public interface IUpsertFoodRatingService
    {
        Task UpsertAsync(string userId, int foodId, int score);
    }
}