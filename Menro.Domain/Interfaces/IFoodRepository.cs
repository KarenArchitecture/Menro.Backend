using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    public interface IFoodRepository
    {
        Task<List<FoodCategory>> GetAllCategoriesAsync();
        Task<List<Food>> GetPopularFoodsByCategoryAsync(int categoryId, int count);
        Task<List<int>> GetAllCategoryIdsAsync();
    }
}
