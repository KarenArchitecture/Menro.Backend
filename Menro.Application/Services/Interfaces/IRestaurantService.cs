using Menro.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<bool> AddRestaurantAsync(RestaurantDto dto);

        Task<IEnumerable<FeaturedRestaurantDto>> GetFeaturedRestaurantsAsync();
    }
}
