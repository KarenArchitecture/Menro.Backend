namespace Menro.Application.Foods.DTOs
{
    public class FoodVariantDto
    {
        public int? Id { get; set; } = null;
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public List<FoodAddonDto>? Addons { get; set; } = new();
    }
}
