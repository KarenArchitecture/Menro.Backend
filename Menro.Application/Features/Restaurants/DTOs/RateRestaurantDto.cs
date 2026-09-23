using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RateRestaurantDto
    {
        [Required]
        public int RestaurantId { get; set; }

        [Range(1, 5, ErrorMessage = "امتیاز باید بین ۱ تا ۵ باشد.")]
        public int Score { get; set; }
    }
}