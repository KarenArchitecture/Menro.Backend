namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RateRestaurantRequestDto
    {
        public int RestaurantId { get; set; }
        public int Score { get; set; } // 1–5
    }

    public class RestaurantRatingResultDto
    {
        public double AverageRating { get; set; }
        public int VotersCount { get; set; }
        public int? MyScore { get; set; }
    }
}