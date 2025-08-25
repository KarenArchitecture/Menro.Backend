using Menro.Application.Features.Order.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Order.Services
{
    public interface IOrderService
    {
        Task<List<MonthlySales>> GetMonthlySalesRawAsync(int? restaurantId = null);
        Task<decimal> GetTotalRevenueAsync(int? restaurantId = null);
        Task<int> GetNewOrdersCountAsync(int? restaurantId = null, int daysBack = 30);

    }
}
