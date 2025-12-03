public class AdPricingSetting
{
    public int Id { get; set; }

    public AdPlacementType PlacementType { get; set; }

    public AdBillingType BillingType { get; set; }


    public int UnitPrice { get; set; }

    public int MinUnits { get; set; }
    public int MaxUnits { get; set; }

    public bool IsActive { get; set; } = true;

}
