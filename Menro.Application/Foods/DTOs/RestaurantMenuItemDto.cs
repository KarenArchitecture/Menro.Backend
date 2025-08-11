using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.DTOs
{
    public class RestaurantMenuItemDto
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Ingredients { get; init; }
        public int Price { get; init; }
        public string ImageUrl { get; init; }
    }
}
