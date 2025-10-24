namespace Menro.Application.Foods.DTOs
{
    public class FoodDetailsDto
    {
        public int Id { get; set; }  // فقط در ویرایش استفاده میشه
        public string Name { get; set; } = string.Empty;
        public string? Ingredients { get; set; }
        public int Price { get; set; } = 0;  // فقط وقتی محصول تنوع نداره
        public string ImageUrl { get; set; } = string.Empty;
        public int? CustomFoodCategoryId { get; set; }
        public bool HasVariants { get; set; } = false; // آیا محصول variant دارد؟
        public List<FoodVariantDetailsDto> Variants { get; set; } = new(); // لیست variant ها
    }
}
