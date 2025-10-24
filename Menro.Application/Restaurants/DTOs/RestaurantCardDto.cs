using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.DTO
{
    public class RestaurantCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public double Rating { get; set; }
        public int Voters { get; set; }
        public int? Discount { get; set; }
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;
        public string? LogoImageUrl { get; set; }
        public bool IsOpen { get; set; }
        public string Slug { get; set; } = string.Empty;
    }
}
