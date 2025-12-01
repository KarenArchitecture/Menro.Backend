using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.DTOs
{
    public class PublicFoodVariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool IsDefault { get; set; } = false;

        public List<PublicFoodAddonDto> Addons { get; set; } = new();
    }

}
