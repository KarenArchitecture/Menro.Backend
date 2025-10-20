using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Identity.Services
{
    // Menro.Application.Features.Identity.Services
    public interface ICurrentUserService
    {
        string? GetUserId();
        Task<int> GetRestaurantIdAsync();
    }


}
