// Application/Features/FoodRatings/DTOs/UpsertFoodRatingResultDto.cs
namespace Menro.Application.Features.FoodRatings.DTOs
{
    public class UpsertFoodRatingResultDto
    {
        public double AverageRating { get; set; }
        public int VotersCount { get; set; }
    }
}