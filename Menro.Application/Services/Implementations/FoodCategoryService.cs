using Menro.Application.Services.Interfaces;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Implementations
{
    public class FoodCategoryService : IFoodCategoryService
    {
        private readonly IUnitOfWork _uow;
        public FoodCategoryService(IUnitOfWork uow)
        {
            _uow = uow;
        }
    }
}
