namespace Menro.Application.Restaurants.DTOs
{
    public class RegisterRestaurantDto
    {

        public string RestaurantName { get; set; } = string.Empty;

        public string RestaurantDescription { get; set; } = string.Empty;

        public string RestaurantAddress { get; set; } = string.Empty;

        public int RestaurantCategoryId { get; set; }

        public TimeSpan RestaurantOpenTime { get; set; }

        public TimeSpan RestaurantCloseTime { get; set; }

        public string OwnerNationalId { get; set; } = string.Empty;

        public string RestaurantAccountNumber { get; set; } = string.Empty;
    }
}
