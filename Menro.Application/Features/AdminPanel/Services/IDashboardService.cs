using Menro.Application.Features.AdminPanel.DTOs;
using Menro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.AdminPanel.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
        Task<decimal> GetTotalRevenueAsync(int? restaurantId = null);
        Task<int> GetThisMonthOrdersCountAsync(int? restaurantId = null);
        Task<int> GetTodayOrdersCountAsync(int? restaurantId = null);
        Task<decimal> GetTodayOrdersRevenueAsync(int? restaurantId = null);
        Task<List<SalesByMonthDto>> GetMonthlySalesAsync(int? restaurantId = null);
    }
}
