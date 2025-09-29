using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.DTOs
{
    public class RestaurantBannerDto
    {
        public string Name { get; set; } = string.Empty;

        public string? BannerImageUrl { get; set; }

        public double AverageRating { get; set; }

        public int VotersCount { get; set; }
    }
}

