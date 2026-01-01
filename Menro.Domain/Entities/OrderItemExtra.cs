using System.ComponentModel.DataAnnotations.Schema;

namespace Menro.Domain.Entities
{
    public class OrderItemExtra
    {
        public int Id { get; set; }

        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = null!;

        // ✅ FK حفظ میشه، ولی nullable برای اینکه تاریخچه سفارش هیچ‌وقت نشکنه
        public int? FoodAddonId { get; set; }
        public FoodAddon? FoodAddon { get; set; }

        // ✅ Snapshot عنوان مخلفات در لحظه سفارش
        public string AddonTitleSnapshot { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraPrice { get; set; }
    }
}
