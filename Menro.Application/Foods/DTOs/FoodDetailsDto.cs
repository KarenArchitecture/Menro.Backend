namespace Menro.Application.Foods.DTOs
{
    public class FoodDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Ingredients { get; set; }
        public int Price { get; set; }

        public string? ImageName { get; set; } // ← جدید، فقط نام فایل
        public string? ImageUrl { get; set; }  // ← برای نمایش

        public int? FoodCategoryId { get; set; }
        public bool HasVariants { get; set; }
        public List<FoodVariantDetailsDto> Variants { get; set; } = new();
    }
}
