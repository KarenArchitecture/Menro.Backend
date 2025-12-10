using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class OrderItemExtra
    {
        public int Id { get; set; }

        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; }

        public int FoodAddonId { get; set; } // the menu addon
        public FoodAddon FoodAddon { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraPrice { get; set; }
    }

}
