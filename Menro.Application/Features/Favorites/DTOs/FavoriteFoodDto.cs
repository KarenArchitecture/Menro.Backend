using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Favorites.DTOs
{
    public class FavoriteFoodDto
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public string RestaurantName { get; set; }
        public string ImageUrl { get; set; }
    }
}
