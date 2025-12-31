using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Ads.DTOs
{
    public class RestaurantAdBannerDto
    {
        public int Id { get; set; } // RestaurantAd.Id
        public int RestaurantId { get; set; }
        public string ImageUrl { get; set; } = ""; // map from ImageFileName via IFileUrlService
        public string RestaurantName { get; set; } = ""; 
        public string Slug { get; set; } = ""; 
        public string CommercialText { get; set; } = ""; 
        public string? TargetUrl { get; set; } // optional (you already store it)
    }
}
