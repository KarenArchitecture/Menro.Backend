using Menro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Contracts
{
    public class SearchHit
    {
        public SearchHitType Type { get; set; }
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string? Subtitle { get; set; }

        // For Restaurant => BannerImageUrl , For Food => Food.ImageUrl
        public string? ImageFileName { get; set; }

        public int? RestaurantId { get; set; }
        public string? RestaurantSlug { get; set; }

        // Restaurant-only extras (optional for Food)
        public string? LogoImageUrl { get; set; }
        public string? Category { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public int Discount { get; set; }
        public double Rating { get; set; }
        public int Voters { get; set; }
        public bool IsOpen { get; set; }

        public int Rank { get; set; }
    }
}
