using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Search.DTOs
{
    public class SearchItemDto
    {
        public SearchItemType Type { get; set; }
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string? Subtitle { get; set; }

        public string? ImageUrl { get; set; }
        public string TargetUrl { get; set; } = "";

        public int? RestaurantId { get; set; }
        public string? RestaurantSlug { get; set; }

        // Restaurant UI fields
        public string? LogoImageUrl { get; set; }
        public string? Category { get; set; }
        public string? OpenTime { get; set; }
        public string? CloseTime { get; set; }
        public int Discount { get; set; }
        public double Rating { get; set; }
        public int Voters { get; set; }
        public bool IsOpen { get; set; }
    }
}
