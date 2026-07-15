using Menro.Application.Features.Foods.DTOs;

namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantMenuDto
    {
        public int CategoryId { get; set; }
        public string CategoryKey { get; set; } = string.Empty;
        public string CategoryTitle { get; set; } = string.Empty;
        public string SvgIcon { get; set; } = string.Empty;
        public List<FoodCardDto> Foods { get; set; } = new();
    }
}
