namespace Menro.Application.Features.Foods.DTOs
{
    public class ShopFoodDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Image { get; set; } = string.Empty;
        public double? Rating { get; set; }
        public int CategoryId { get; set; }
    }
}
