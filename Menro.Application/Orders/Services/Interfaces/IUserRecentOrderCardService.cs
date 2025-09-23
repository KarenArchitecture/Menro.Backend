// Menro.Application/Orders/Services/Interfaces/IUserRecentOrderCardService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Menro.Application.Foods.DTOs;
using Menro.Application.Orders.DTOs;

namespace Menro.Application.Orders.Services.Interfaces
{
    public interface IUserRecentOrderCardService
    {
        /// <summary>
        /// Most recently-ordered foods for this user (deduped by Food),
        /// ordered by last ordered date desc, limited by count.
        /// </summary>
        Task<List<RecentOrdersFoodCardDto>> GetUserRecentOrderedFoodsAsync(string userId, int count = 8);
    }
}
