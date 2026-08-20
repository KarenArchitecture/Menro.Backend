namespace Menro.Application.Features.Restaurants.DTOs
{
    public class MyRestaurantStatusDto
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public int Status { get; set; } // 1=Pending, 2=Approved, 3=Rejected
        public string? RejectReason { get; set; }
    }
}
