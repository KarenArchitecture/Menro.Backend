using Menro.Domain.Enums;

namespace Menro.Infrastructure.Data.Seed.Demo.Data;

public static class DemoDiscountSeedData
{
    public static IReadOnlyCollection<decimal> PercentPool =>
        new List<decimal>
        {
            10,
            15,
            20,
            25,
            30
        };

    public const double RestaurantDiscountChance = 0.35;

    public const double FoodDiscountChance = 0.5;

    public const int MinFoodsPerRestaurant = 1;

    public const int MaxFoodsPerRestaurant = 3;

    public const int MinStartDaysAgo = 0;

    public const int MaxStartDaysAgo = 3;

    public const int MinEndDays = 5;

    public const int MaxEndDays = 15;
}