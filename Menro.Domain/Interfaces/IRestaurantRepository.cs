using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Interfaces
{
    public interface IRestaurantRepository : IRepository<Restaurant>
    {
        //Featured Restaurants Carousel
        Task<IEnumerable<Restaurant>> GetFeaturedRestaurantsAsync();

        //Random Restaurants Cards
        Task<List<Restaurant>> GetAllActiveApprovedWithDetailsAsync();
    }
}
