namespace Menro.Application.Features.Foods.DTOs
{
    public class PublicFoodVariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool IsDefault { get; set; } = false;

        public List<PublicFoodAddonDto> Addons { get; set; } = new();
    }

}
