using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    public interface IFoodCategoryRepository : IRepository<FoodCategory>
    {
        Task<IEnumerable<FoodCategory>> GetByRestaurantSlugAsync(string restaurantSlug);
    }
}
