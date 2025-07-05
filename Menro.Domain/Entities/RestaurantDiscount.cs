using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class RestaurantDiscount
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

        public int? FoodId { get; set; } // null = general restaurant-wide discount
        public Food? Food { get; set; }

        [Range(0, 100, ErrorMessage = "درصد تخفیف باید بین 0 تا 100 باشد")]
        public int Percent { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
