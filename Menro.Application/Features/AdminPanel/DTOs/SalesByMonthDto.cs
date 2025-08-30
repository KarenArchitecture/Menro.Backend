using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.AdminPanel.DTOs
{
    public class SalesByMonthDto
    {
        public int Month { get; set; }          // 1..12 (میلادی)
        public decimal TotalSales { get; set; } // مجموع مبلغ فروش در آن ماه
    }
}
