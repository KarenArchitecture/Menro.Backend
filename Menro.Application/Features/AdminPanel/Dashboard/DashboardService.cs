using Menro.Application.Features.AdminPanel.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.AdminPanel.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private IUnitOfWork _uow;
        public DashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _uow.Order.GetTotalRevenueAsync();
        }
        public async Task<int> GetNewOrdersCountAsync(int? restaurantId = null)
        {
            var since = DateTime.UtcNow.AddDays(-17).ToLocalTime(); // داخل پرانتز => تا فلان روز قبل تر
            var query = _uow.Order.Query();

            if (restaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantId.Value);

            return await query.CountAsync(o => o.CreatedAt >= since);
        }
        public async Task<List<SalesByMonthDto>> GetMonthlySalesAsync(int? restaurantId = null)
        {
            var year = DateTime.UtcNow.Year;

            var q = _uow.Order.Query()
                .Where(o => o.Status == OrderStatus.Completed && o.CreatedAt.Year == year);

            if (restaurantId.HasValue)
                q = q.Where(o => o.RestaurantId == restaurantId.Value);

            var list = await q
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new SalesByMonthDto
                {
                    Month = g.Key,
                    TotalSales = g.Sum(x => x.TotalAmount)
                })
                .ToListAsync();

            // پر کردن ماه‌های خالی
            return Enumerable.Range(1, 12)
                .GroupJoin(list, m => m, x => x.Month, (m, g) =>
                    g.FirstOrDefault() ?? new SalesByMonthDto { Month = m, TotalSales = 0 })
                .OrderBy(x => x.Month)
                .ToList();
        }


    }
}
