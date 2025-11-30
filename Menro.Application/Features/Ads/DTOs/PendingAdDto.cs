public class PendingAdDto
{
    public int Id { get; set; }
    public string RestaurantName { get; set; } = "";
    public string Placement { get; set; } = "";
    public string Billing { get; set; } = "";
    public int Cost { get; set; }
    public int PurchasedUnits { get; set; }
    public string TargetUrl { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string CommercialText { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
