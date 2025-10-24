using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.Services.Implementations
{
    public class ShopFoodsService : IShopFoodsService
    {
        private readonly IFoodRepository _foodRepository;

        public ShopFoodsService(IFoodRepository foodRepository)
        {
            _foodRepository = foodRepository;
        }

        //public async Task<List<ShopFoodDto>> GetByCategoryIdsAsync(List<int> categoryIds)
        //{
        //    var foods = await _foodRepository.GetByCategoryIdsAsync(categoryIds);
        //    return foods.Select(f => new ShopFoodDto
        //    {
        //        Id = f.Id,
        //        Name = f.Name,
        //        Description = f.Description,
        //        Price = f.Price,
        //        Image = f.Image,
        //        Rating = f.Rating,
        //        CategoryId = f.CategoryId
        //    }).ToList();
        //}
    }
}
