using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.DTOs
{
    public class FeaturedRestaurantDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? CarouselImageUrl { get; set; }
    }
}
