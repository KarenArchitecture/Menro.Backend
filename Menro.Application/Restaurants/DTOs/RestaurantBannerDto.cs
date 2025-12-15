using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.DTOs
{
    public class RestaurantBannerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BannerImageUrl { get; set; } = "/img/res-slider.png"; // fallback
        public double AverageRating { get; set; }
        public int VotersCount { get; set; }
        public int TableCount { get; set; } = 0;

    }
}

