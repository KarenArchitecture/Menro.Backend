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
        public string Name { get; set; } = default!;
        public string SvgIcon { get; set; } = string.Empty;
    }
}
