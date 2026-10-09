using Menro.Domain.Entities;

namespace Menro.Application.Features.Foods.Helpers
{
    public static class FoodPricing
    {
        // همون قاعده‌ی RestaurantMenuService:
        // واریانت پیش‌فرض → اولین واریانت فعال → اولین واریانت → قیمت پایه‌ی غذا
        public static int GetDisplayPrice(Food f)
        {
            if (f.Variants == null || !f.Variants.Any())
                return f.Price;

            var v = f.Variants.FirstOrDefault(x => x.IsDefault == true)
                    ?? f.Variants.FirstOrDefault(x => x.IsAvailable)
                    ?? f.Variants.First();

            return v.Price;
        }
    }
}