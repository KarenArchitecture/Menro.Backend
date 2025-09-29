using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.FoodCategories.DTOs
{
    public class ShopFoodCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SvgIcon { get; set; } = string.Empty;
        public int? GlobalFoodCategoryId { get; set; }
        //public List<ShopFoodDto> Foods { get; set; } = new();
    }
}
