using Menro.Application.DTO;
using Menro.Application.Restaurants.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<bool> AddRestaurantAsync(RestaurantDto dto);
    }
}
