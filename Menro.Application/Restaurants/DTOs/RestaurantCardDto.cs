using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.DTO
{
    public class RestaurantCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string BannerImageUrl { get; set; }
        public string OpeningHours { get; set; }
        public double? AverageRating { get; set; }
        public int VoterCount { get; set; }
        public int? MaxDiscount { get; set; }
    }
}
