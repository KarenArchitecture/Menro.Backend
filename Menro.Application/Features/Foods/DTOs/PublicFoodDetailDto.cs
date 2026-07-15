namespace Menro.Application.Features.Foods.DTOs
{
    public class PublicFoodDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Ingredients { get; set; }
        public int BasePrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public double AverageRating { get; set; }
        public int VotersCount { get; set; }

        public List<PublicFoodVariantDto> Variants { get; set; } = new();
    }

}
