using Menro.Domain.Enums;

namespace Menro.Application.Restaurants.DTOs
{
    public class UpdateRestaurantStatusDto
    {
        public int RestaurantId { get; set; }
        public RestaurantStatus Status { get; set; }
        public string? RejectReason { get; set; }

    }
}
