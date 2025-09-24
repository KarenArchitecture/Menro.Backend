using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.DTOs
{
    public class RestaurantAdBannerDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string RestaurantName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? CommercialText { get; set; }
    }
}
