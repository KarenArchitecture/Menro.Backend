using Menro.Domain.Entities;

namespace Menro.Infrastructure.Data.Seed.Core.Data;

public sealed class GlobalFoodCategorySeedData
{
    private GlobalFoodCategorySeedData() { }

    public static IReadOnlyCollection<GlobalFoodCategory> Data { get; } =
        new List<GlobalFoodCategory>
        {
            new()
            {
                Name = "پیتزا",
                IconId = 34,
                IsActive = true,
                DisplayOrder = 1
            },

            new()
            {
                Name = "برگر",
                IconId = 3,
                IsActive = true,
                DisplayOrder = 2
            },

            new()
            {
                Name = "نوشیدنی گرم",
                IconId = 22,
                IsActive = true,
                DisplayOrder = 3
            },

            new()
            {
                Name = "نوشیدنی سرد",
                IconId = 13,
                IsActive = true,
                DisplayOrder = 4
            },

            new()
            {
                Name = "سالاد",
                IconId = 39,
                IsActive = true,
                DisplayOrder = 5
            },

            new()
            {
                Name = "دسر",
                IconId = 30,
                IsActive = true,
                DisplayOrder = 6
            }
        };
}