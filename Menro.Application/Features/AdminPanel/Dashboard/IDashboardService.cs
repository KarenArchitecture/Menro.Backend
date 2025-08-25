using Menro.Application.Features.AdminPanel.DTOs;
using Menro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.AdminPanel.Dashboard
{
    public interface IDashboardService
    {
        Task<AdminDto> GetAdminDetailsAsync(string userId);
        Task<decimal> GetTotalRevenueAsync(int? restaurantId = null);
        Task<int> GetNewOrdersCountAsync(int? restaurantId = null);
        Task<List<SalesByMonthDto>> GetMonthlySalesAsync(int? restaurantId = null);
        Task<int?> GetRestaurantIdByUserIdAsync(string userId);
    }
}
