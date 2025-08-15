using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int FoodId { get; set; }
        public Food Food { get; set; }

        // تعداد سفارش‌شده
        public int Quantity { get; set; }

        // قیمت در لحظه سفارش (برای جلوگیری از تغییر با تغییر قیمت Food)
        public decimal UnitPrice { get; set; }
    }
}
