using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class FoodCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } // مثل "نوشیدنی سرد"، "پیتزا"

        // ارتباط با جدول رستوران
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;
        public ICollection<Food> Foods { get; set; } = new List<Food>();
    }
}
