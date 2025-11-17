using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Menro.Domain.Entities
{
    public class RestaurantAd
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        public AdPlacementType PlacementType { get; set; }   // اسلایدر یا بنر تمام صفحه
        public AdBillingType BillingType { get; set; }       // بر اساس روز یا کلیک
        public int Cost { get; set; }  // مبلغ نهایی پرداخت شده

        public string ImageFileName { get; set; } = string.Empty;
        public string? TargetUrl { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int PurchasedUnits { get; set; } // تعداد روز یا تعداد کلیک
        public int ConsumedUnits { get; set; }  // روز مصرف‌شده یا کلیک‌های انجام‌شده

        [MaxLength(200)]
        public string? CommercialText { get; set; }

        public bool IsPaused { get; set; } = false;

        [NotMapped]
        public bool IsActive =>
            !IsPaused &&
            DateTime.UtcNow >= StartDate &&
            DateTime.UtcNow <= EndDate &&
            (BillingType == AdBillingType.PerDay || ConsumedUnits < PurchasedUnits);
    }
}
