using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menro.Domain.Enums;


namespace Menro.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        // کاربر سفارش‌دهنده
        public string UserId { get; set; }
        public User User { get; set; }

        // رستورانی که سفارش بهش مربوطه
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

        // مبلغ کل
        public decimal TotalAmount { get; set; }

        // وضعیت سفارش
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // آیتم‌های سفارش
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
