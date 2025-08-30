using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Order.DTOs
{
    public class MonthlySales
    {
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
