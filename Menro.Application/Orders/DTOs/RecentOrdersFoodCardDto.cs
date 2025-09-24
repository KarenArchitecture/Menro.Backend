using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Orders.DTOs
{
    public class RecentOrdersFoodCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double Rating { get; set; }  // 0-5 average
        public int Voters { get; set; }     // ratings count

        // for CTA building → slug → id → search
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string? RestaurantSlug { get; set; }
    }
}
