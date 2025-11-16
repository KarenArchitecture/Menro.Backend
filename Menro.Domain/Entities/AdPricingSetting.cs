public class AdPricingSetting
{
    public int Id { get; set; }

    public AdPlacementType PlacementType { get; set; }
    public AdBillingType BillingType { get; set; }

    public int BasePrice { get; set; }     // قیمت پایه
    public int UnitPrice { get; set; }     // قیمت هر روز / هر کلیک

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
