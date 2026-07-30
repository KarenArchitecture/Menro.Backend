namespace Menro.Application.Features.Ads.DTOs
{
    public class MyAdItemDto
    {
        public int Id { get; set; }
        public string Placement { get; set; } = string.Empty;   // "MainSlider" / ...
        public string Billing { get; set; } = string.Empty;     // "PerDay" / "PerClick" / "PerView"
        public int Cost { get; set; }
        public int PurchasedUnits { get; set; }
        public int ConsumedUnits { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string CommercialText { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;      // "Pending" / "Approved" / "Rejected"
        public string? AdminNotes { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedAtShamsi { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartDateShamsi { get; set; } = string.Empty;
        public string EndDateShamsi { get; set; } = string.Empty;
    }
}