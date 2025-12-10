using Menro.Domain.Entities;

namespace Menro.Infrastructure.Seed
{
    public static class GlobalFoodCategorySeed
    {
        public static readonly GlobalFoodCategory[] Data = new[]
        {
            new GlobalFoodCategory { Name = "پیتزا",        IconId = 34,     IsActive = true, DisplayOrder = 1 },
            new GlobalFoodCategory { Name = "برگر",         IconId = 3,    IsActive = true, DisplayOrder = 2 },
            new GlobalFoodCategory { Name = "نوشیدنی گرم",  IconId = 22,  IsActive = true, DisplayOrder = 3 },
            new GlobalFoodCategory { Name = "نوشیدنی سرد",  IconId = 13, IsActive = true, DisplayOrder = 4 },
            new GlobalFoodCategory { Name = "سالاد",        IconId = 39,     IsActive = true, DisplayOrder = 5 },
            new GlobalFoodCategory { Name = "دسر",          IconId = 30,   IsActive = true, DisplayOrder = 6 },
        };
    }
        
}