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
            throw new NotImplementedException();
            //return await _uow.Order
            //    .Where(o => o.Status == OrderStatus.Completed) // فقط سفارش‌های تکمیل‌شده
            //    .SumAsync(o => o.TotalAmount); // جمع ستون مبلغ کل سفارش
        }

    }
}
