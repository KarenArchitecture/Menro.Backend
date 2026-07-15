namespace Menro.Application.Features.Foods.DTOs
{
    public class MenuListFoodDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public double AverageRating { get; set; }
        public int VotersCount { get; set; }

        // Category info (Custom or Global)
        public int? CustomFoodCategoryId { get; set; }
        public string? CustomFoodCategoryName { get; set; }
        public int? CustomFoodCategorySvg { get; set; }

        public int? GlobalFoodCategoryId { get; set; }
        public string? GlobalFoodCategoryName { get; set; }
        public int? GlobalFoodCategorySvg { get; set; }
    }
}
