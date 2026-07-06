using Menro.Application.Features.Favorites.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Favorites.Services.Interfaces
{
    public interface IFavoriteFoodService
    {
        Task ToggleAsync(string userId, int foodId);

        Task<List<FavoriteFoodDto>> GetUserFavoritesAsync(string userId);

        Task<List<int>> GetFavoriteFoodIdsAsync(string userId);
    }
}
