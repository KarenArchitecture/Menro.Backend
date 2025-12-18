using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Ads.DTOs
{
    public class RestaurantAdCarouselDto
    {
        public int Id { get; set; }                  // RestaurantId (same as before)
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string CarouselImageUrl { get; set; } = ""; // map from RestaurantAd.ImageFileName via IFileUrlService
        public int AdId { get; set; }                // optional but recommended for future PerClick tracking on carousel
    }
}
