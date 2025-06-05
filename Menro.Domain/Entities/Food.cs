using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string ImageUrl { get; set; }

        // ارتباط با رستوران
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;
        // ارتباط با دسته بندی غذاها
        public int FoodCategoryId { get; set; }
        public FoodCategory Category { get; set; } = null!;
    }
}
