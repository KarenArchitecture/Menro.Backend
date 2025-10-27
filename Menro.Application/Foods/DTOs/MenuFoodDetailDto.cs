using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.DTOs
{
    public class MenuFoodDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Price { get; set; }
        public string? Ingredients { get; set; }
        public bool IsAvailable { get; set; }
        public double AverageRating { get; set; }
        public int VotersCount { get; set; }

        public List<MenuFoodVariantDto> Variants { get; set; } = new();

        // Category info
        public int? CustomFoodCategoryId { get; set; }
        public string? CustomFoodCategoryName { get; set; }
        public int? CustomFoodCategorySvg { get; set; }

        public int? GlobalFoodCategoryId { get; set; }
        public string? GlobalFoodCategoryName { get; set; }
        public int? GlobalFoodCategorySvg { get; set; }
    }

    public class MenuFoodVariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public List<MenuFoodAddonDto> Addons { get; set; } = new();
    }

    public class MenuFoodAddonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ExtraPrice { get; set; }
    }
}
