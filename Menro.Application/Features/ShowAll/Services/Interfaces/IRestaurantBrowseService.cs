using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.ShowAll.DTOs;

namespace Menro.Application.Features.ShowAll.Services.Interfaces
{
    public interface IRestaurantBrowseService
    {
        Task<PagedResultDto<RestaurantCardDto>> GetRestaurantsPageAsync(int take = 20, int? cursorId = null);
    }
}
