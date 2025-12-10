namespace Menro.Application.Restaurants.DTOs
{
    public class RestaurantListForAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}
