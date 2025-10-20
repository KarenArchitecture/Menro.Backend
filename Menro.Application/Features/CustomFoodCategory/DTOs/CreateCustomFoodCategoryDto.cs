using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.CustomFoodCategory.DTOs
{
    public class CreateCustomFoodCategoryDto
    {
        public string Name { get; set; }
        public string SvgIcon { get; set; } = string.Empty;
    }
}
