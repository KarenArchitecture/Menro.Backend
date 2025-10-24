namespace Menro.Application.Foods.DTOs
{
    public class CreateFoodDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Ingredients { get; set; }
        public int Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        // NEW: support both category types
        public int? CustomFoodCategoryId { get; set; }   // restaurant-local category
        public int? GlobalFoodCategoryId { get; set; }   // global admin category
        public bool HasVariants { get; set; }
        public List<FoodVariantDto>? Variants { get; set; }
    }

}
