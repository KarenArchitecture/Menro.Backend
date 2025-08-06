using Menro.Application.Foods.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.DTOs
{
    public class RestaurantMenuDto
    {
        public int CategoryId { get; set; }
        public string CategoryKey { get; set; } = string.Empty;
        public string CategoryTitle { get; set; } = string.Empty;
        public string SvgIcon { get; set; } = string.Empty;
        public List<FoodCardDto> Foods { get; set; } = new();
    }
}
