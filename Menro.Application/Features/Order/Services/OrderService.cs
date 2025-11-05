using Menro.Application.Features.Order.DTOs;
using Menro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Order.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        // ✅
        public async Task<decimal> GetTotalRevenueAsync(int? restaurantId = null)
        {
            return await _orderRepository.GetTotalRevenueAsync(restaurantId);
        }
        // ✅
        public async Task<List<MonthlySalesDto>> GetMonthlySalesRawAsync(int? restaurantId = null)
        {
            var year = DateTime.UtcNow.Year;
            var orders = await _orderRepository.GetCompletedOrdersAsync(restaurantId, year);

            var grouped = orders
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new MonthlySalesDto
                {
                    Month = g.Key,
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .ToList();

            // پر کردن ماه‌های خالی
            return Enumerable.Range(1, 12)
                .GroupJoin(grouped, m => m, x => x.Month, (m, g) =>
                    g.FirstOrDefault() ?? new MonthlySalesDto { Month = m, TotalAmount = 0 })
                .OrderBy(x => x.Month)
                .ToList();
        }
        public async Task<int> GetRecentOrdersCountAsync(int? restaurantId = null, int daysBack = 0)
        {
            DateTime since;

            if (daysBack == 0)
            {
                since = DateTime.UtcNow.Date;
            }
            else
            {
                since = DateTime.UtcNow.AddDays(-daysBack);
            }

            return await _orderRepository.GetRecentOrdersCountAsync(restaurantId, since);
        }
        public async Task<decimal> GetRecentOrdersRevenueAsync(int? restaurantId = null, int daysBack = 0)
        {
            DateTime since;

            if (daysBack == 0)
            {
                since = DateTime.UtcNow.Date;
            }
            else
            {
                since = DateTime.UtcNow.AddDays(-daysBack);
            }

            return await _orderRepository.GetRecentOrdersRevenueAsync(restaurantId, since);
        }



    }
}
