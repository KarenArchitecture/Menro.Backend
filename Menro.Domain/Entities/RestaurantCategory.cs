using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class RestaurantCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } // مثل "کافه"، "فست‌فودی"

        // Navigation
        public ICollection<Restaurant> Restaurants { get; set; }

    }
}
