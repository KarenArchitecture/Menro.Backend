using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Favorites.DTOs
{
    public class FavoriteFoodDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public int Price { get; set; }

        public double Rating { get; set; }

        public int Voters { get; set; }

        public string RestaurantName { get; set; } = string.Empty;

        public int RestaurantId { get; set; }

        public string RestaurantSlug { get; set; } = string.Empty;
    }
}
