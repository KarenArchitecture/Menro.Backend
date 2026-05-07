using Menro.Application.DTO;
using Menro.Application.Features.ShowAll.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.ShowAll.Services.Interfaces
{
    public interface IRestaurantBrowseService
    {
        Task<PagedResultDto<RestaurantCardDto>> GetRestaurantsPageAsync(int take = 20, int? cursorId = null);
    }
}
