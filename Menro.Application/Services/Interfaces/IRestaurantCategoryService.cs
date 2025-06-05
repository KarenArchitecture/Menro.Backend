using Menro.Application.DTO;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Interfaces
{
    public interface IRestaurantCategoryService
    {
        Task<List<RestaurantCategoryDto>> GetAllCategoriesAsync();
    }
}
