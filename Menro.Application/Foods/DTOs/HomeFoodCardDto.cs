using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.DTOs
{
    public class HomeFoodCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public double Rating { get; set; }
        public int Voters { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
    }
}
