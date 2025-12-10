using Menro.Application.Restaurants.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Services.Interfaces
{
    public interface IRestaurantBannerService
    {
        Task<RestaurantBannerDto?> GetBannerBySlugAsync(string slug);

        // Optional: call this after owner updates the banner
        void InvalidateCache(string slug);
    }
}
