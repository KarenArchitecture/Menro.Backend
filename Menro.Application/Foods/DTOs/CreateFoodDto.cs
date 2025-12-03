namespace Menro.Application.Foods.DTOs
{
    public class CreateFoodDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Ingredients { get; set; }
        public int Price { get; set; }

        public string? ImageName { get; set; } // ← تغییر کرد!

        public int FoodCategoryId { get; set; }
        public bool HasVariants { get; set; }
        public List<FoodVariantDto>? Variants { get; set; }
    }
}
