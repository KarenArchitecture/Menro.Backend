using Menro.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Services.Interfaces
{
    public interface IUserRecentOrderCardService
    {
        Task<List<RestaurantCardDto>> GetRecentOrderedRestaurantCardsAsync(string userId, int count = 8);
    }
}
