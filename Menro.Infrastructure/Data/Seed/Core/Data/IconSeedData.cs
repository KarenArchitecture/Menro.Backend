using Menro.Domain.Entities;

namespace Menro.Infrastructure.Data.Seed.Core.Data;

public sealed class IconSeedData
{
    private IconSeedData() { }

    public static IReadOnlyCollection<Icon> Data { get; } =
        new List<Icon>
        {
            new() { FileName = "AppetizerIcon.svg", Label = "Appetizer" },
            new() { FileName = "BreadIcon.svg", Label = "Bread" },
            new() { FileName = "BurgerIcon.svg", Label = "Burger" },
            new() { FileName = "CakeIcon.svg", Label = "Cake" },
            new() { FileName = "CakeIcon2.svg", Label = "Cake 2" },
            new() { FileName = "CandyIcon.svg", Label = "Candy" },
            new() { FileName = "CarrotIcon.svg", Label = "Carrot" },
            new() { FileName = "ChickenIcon.svg", Label = "Chicken" },
            new() { FileName = "ChineseFoodIcon.svg", Label = "Chinese Food" },
            new() { FileName = "ChineseFoodIcon2.svg", Label = "Chinese Food 2" },
            new() { FileName = "ChocolateIcon.svg", Label = "Chocolate" },
            new() { FileName = "ColaIcon.svg", Label = "Cola" },
            new() { FileName = "ColdDrinksIcon.svg", Label = "Cold Drinks" },
            new() { FileName = "CupCakeIcon.svg", Label = "Cup Cake" },
            new() { FileName = "CupCakeIcon2.svg", Label = "Cup Cake 2" },
            new() { FileName = "DonutIcon.svg", Label = "Donut" },
            new() { FileName = "EggIcon.svg", Label = "Egg" },
            new() { FileName = "FastFoodIcon.svg", Label = "Fast Food" },
            new() { FileName = "FishIcon.svg", Label = "Fish" },
            new() { FileName = "FrenchFriesIcon.svg", Label = "French Fries" },
            new() { FileName = "HotDrinksIcon.svg", Label = "Hot Drinks" },
            new() { FileName = "IceCreamIcon.svg", Label = "Ice Cream" },
            new() { FileName = "IceCreamIcon2.svg", Label = "Ice Cream 2" },
            new() { FileName = "IceIcon.svg", Label = "Ice" },
            new() { FileName = "IranianFoodIcon.svg", Label = "Iranian Food" },
            new() { FileName = "KababIcon.svg", Label = "Kabab" },
            new() { FileName = "KababIcon2.svg", Label = "Kabab 2" },
            new() { FileName = "LasagnaIcon.svg", Label = "Lasagna" },
            new() { FileName = "MainCourseIcon.svg", Label = "Main Course" },
            new() { FileName = "MeatIcon.svg", Label = "Meat" },
            new() { FileName = "MeatIcon2.svg", Label = "Meat 2" },
            new() { FileName = "PancakeIcon.svg", Label = "Pancake" },
            new() { FileName = "PizzaFullIcon.svg", Label = "Pizza Full" },
            new() { FileName = "PizzaSliceIcon.svg", Label = "Pizza Slice" },
            new() { FileName = "PopsicleIcon.svg", Label = "Popsicle" },
            new() { FileName = "RiceIcon.svg", Label = "Rice" },
            new() { FileName = "RicePotIcon.svg", Label = "Rice Pot" },
            new() { FileName = "SaladIcon.svg", Label = "Salad" },
            new() { FileName = "SaladIcon2.svg", Label = "Salad 2" },
            new() { FileName = "SaladIcon3.svg", Label = "Salad 3" },
            new() { FileName = "SeaFoodIcon.svg", Label = "Sea Food" },
            new() { FileName = "SeaFoodIcon2.svg", Label = "Sea Food 2" },
            new() { FileName = "SoupIcon.svg", Label = "Soup" },
            new() { FileName = "SoupIcon2.svg", Label = "Soup 2" },
            new() { FileName = "SpaghettiIcon.svg", Label = "Spaghetti" },
            new() { FileName = "TakeoutDrinkIcon.svg", Label = "Takeout Drink" },
            new() { FileName = "TakeoutIcon.svg", Label = "Takeout" },
            new() { FileName = "VegetablesIcon.svg", Label = "Vegetables" },
            new() { FileName = "WaffleIcon.svg", Label = "Waffle" },
            new() { FileName = "WaterIcon.svg", Label = "Water" }
        };
}