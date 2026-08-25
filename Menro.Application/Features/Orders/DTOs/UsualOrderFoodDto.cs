namespace Menro.Application.Features.Orders.DTOs
{
    public class UsualOrderFoodDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Price { get; set; }
        public double Rating { get; set; }
        public int VotersCount { get; set; }
        public List<UsualOrderVariantDto> Variants { get; set; } = new();
    }

    public class UsualOrderVariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool IsDefault { get; set; }
        public List<UsualOrderAddonDto> Addons { get; set; } = new();
    }

    public class UsualOrderAddonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ExtraPrice { get; set; }
    }
}