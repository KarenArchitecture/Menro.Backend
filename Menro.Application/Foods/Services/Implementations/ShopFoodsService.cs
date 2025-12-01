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

    }
}
