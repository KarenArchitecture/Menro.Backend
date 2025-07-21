using Menro.Application.Restaurants.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Services.Interfaces
{
    public interface IRestaurantShopBannerService
    {
        Task<RestaurantShopBannerDto?> GetShopBannerAsync(string slug);
    }
}
