using Menro.Application.Features.Restaurants.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Restaurants.Services.Interfaces
{
    public interface IRandomRestaurantCardService
    {
        Task<List<RestaurantCardDto>> GetRandomRestaurantCardsAsync(int count = 8);

    }
}
