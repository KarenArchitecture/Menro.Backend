using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Infrastructure.Repositories
{
    public class RestaurantCategoryRepository : Repository<RestaurantCategory>, IRestaurantCategoryRepository
    {
        private readonly MenroDbContext _context;

        public RestaurantCategoryRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
