using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menro.Infrastructure.Data;

namespace Menro.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly MenroDbContext _context;

        public OrderRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }


    }
}
