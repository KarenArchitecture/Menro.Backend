using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.FoodCategories.DTOs
{
    public class RestaurantFoodCategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SvgIcon { get; set; } = string.Empty;
        public bool IsGlobal { get; set; } // true if it's a global category
    }
}
