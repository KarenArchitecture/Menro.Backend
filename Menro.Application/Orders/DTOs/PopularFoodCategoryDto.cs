using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Orders.DTOs
{
    public class PopularFoodCategoryDto
    {
        public string CategoryTitle { get; set; } = string.Empty;
        public List<RecentOrdersFoodCardDto> Foods { get; set; } = new();
        public string SvgIcon { get; set; } = string.Empty;
    }
}
