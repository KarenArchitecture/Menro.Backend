// Application/Features/FoodCombos/DTOs/PublicComboFoodDto.cs
namespace Menro.Application.Features.FoodCombos.DTOs
{
    public class PublicComboFoodDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Price { get; set; }
        public double Rating { get; set; }
        public int VotersCount { get; set; }
        public List<PublicComboVariantDto> Variants { get; set; } = new();
    }

    public class PublicComboVariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool IsDefault { get; set; }
        public List<PublicComboAddonDto> Addons { get; set; } = new();
    }

    public class PublicComboAddonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ExtraPrice { get; set; }
    }
}