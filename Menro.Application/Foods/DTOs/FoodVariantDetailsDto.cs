namespace Menro.Application.Foods.DTOs
{
    public class FoodVariantDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool? IsDefault { get; set; } = false;
        public List<FoodAddonDetailsDto> Addons { get; set; } = new();

    }
}
