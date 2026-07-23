using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Search.DTOs;

namespace Menro.Application.Features.Search.Services.Interfaces
{
    public interface IRestaurantBrowseService
    {
        Task<PagedResultDto<RestaurantCardDto>> GetRestaurantsPageAsync(int take = 20, int? cursorId = null);
    }
}
