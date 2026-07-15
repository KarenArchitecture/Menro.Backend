using Menro.Domain.Enums;

namespace Menro.Application.Features.Restaurants.DTOs
{
    public class UpdateRestaurantStatusDto
    {
        public int RestaurantId { get; set; }
        public RestaurantStatus Status { get; set; }
        public string? RejectReason { get; set; }

    }
}
