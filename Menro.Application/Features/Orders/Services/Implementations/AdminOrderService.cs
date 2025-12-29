using Menro.Application.Features.Orders.DTOs;
using Menro.Domain.Interfaces;
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Orders.Services.Interfaces;

namespace Menro.Application.Features.Orders.Services.Implementations
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IGlobalDateTimeService _dateTimeService;
        public AdminOrderService(IOrderRepository orderRepository,
            IGlobalDateTimeService dateTimeService)
        {
            _orderRepository = orderRepository;
            _dateTimeService = dateTimeService;
        }

        /* dashboard stats */

        public async Task<decimal> GetTotalRevenueAsync(int? restaurantId = null)
        {
            return await _orderRepository.GetTotalRevenueAsync(restaurantId);
        }
        public async Task<List<MonthlySalesDto>> GetMonthlySalesRawAsync(int? restaurantId = null)
        {
            var persianYear = _dateTimeService.GetPersianYear(DateTime.UtcNow);

            // شروع و پایان سال شمسی فعلی
            DateTime startOfYear = _dateTimeService.ConvertToGregorian(persianYear, 1, 1);
            DateTime startOfNextYear = _dateTimeService.ConvertToGregorian(persianYear + 1, 1, 1);

            //var year = DateTime.UtcNow.Year;
            var orders = await _orderRepository.GetCompletedOrdersAsync(restaurantId, startOfYear, startOfNextYear);

            var grouped = orders
                .GroupBy(o => _dateTimeService.GetPersianMonth(o.CreatedAt))
                .Select(g => new MonthlySalesDto
                {
                    Month = g.Key,
                    MonthName = _dateTimeService.GetPersianMonthName(g.First().CreatedAt),
                    TotalAmount = g.Sum(x => x.TotalPrice)
                })
                .ToList();

            var result = Enumerable.Range(1, 12)
                .GroupJoin(grouped, m => m, x => x.Month, (m, g) =>
                    g.FirstOrDefault() ?? new MonthlySalesDto
                    {
                        Month = m,
                        MonthName = _dateTimeService.GetPersianMonthName(
                            _dateTimeService.ConvertToGregorian(persianYear, m, 1)
                        ),
                        TotalAmount = 0
                    })
                .OrderBy(x => x.Month)
                .ToList();

            return result;
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

        /* order management */
        public async Task<List<AdminOrderListItemDto>> GetActiveOrdersAsync(int restaurantId)
        {
            var orders = await _orderRepository.GetActiveOrdersAsync(restaurantId);

            return orders.Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                RestaurantOrderNumber = o.RestaurantOrderNumber,
                TableNumber = o.TableNumber,
                TotalPrice = o.TotalPrice,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            }).ToList();
        }

        public async Task<List<AdminOrderListItemDto>> GetOrderHistoryAsync(int restaurantId)
        {
            var orders = await _orderRepository.GetOrderHistoryAsync(restaurantId);

            return orders.Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                RestaurantOrderNumber = o.RestaurantOrderNumber,
                TableNumber = o.TableNumber,
                TotalPrice = o.TotalPrice,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            }).ToList();
        }
    }
}
