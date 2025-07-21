using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.DTOs
{
    public class PopularFoodCategoryDto
    {
        public string CategoryTitle { get; set; } = string.Empty;
        public List<FoodCardDto> Foods { get; set; } = new List<FoodCardDto>();
        public string SvgIcon { get; set; } = string.Empty;
    }
}
