using Menro.Application.Foods.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Orders.DTOs
{
    public class PopularFoodsDto
    {
        public string CategoryTitle { get; set; } = string.Empty;
        public int? IconId { get; set; } = 0;
        public List<HomeFoodCardDto> Foods { get; set; } = new();
    }
}
