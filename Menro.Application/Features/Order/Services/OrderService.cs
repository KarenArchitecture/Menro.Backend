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
        public async Task<List<MonthlySales>> GetMonthlySalesRawAsync(int? restaurantId = null)
        {
            var year = DateTime.UtcNow.Year;
            var orders = await _orderRepository.GetCompletedOrdersAsync(restaurantId, year);

            var grouped = orders
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new MonthlySales
                {
                    Month = g.Key,
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .ToList();

            // پر کردن ماه‌های خالی
            return Enumerable.Range(1, 12)
                .GroupJoin(grouped, m => m, x => x.Month, (m, g) =>
                    g.FirstOrDefault() ?? new MonthlySales { Month = m, TotalAmount = 0 })
                .OrderBy(x => x.Month)
                .ToList();
        }
        public async Task<int> GetNewOrdersCountAsync(int? restaurantId = null, int daysBack = 30)
        {
            var since = DateTime.UtcNow.AddDays(-daysBack);
            return await _orderRepository.CountNewOrdersAsync(restaurantId, since);
        }

    }
}
