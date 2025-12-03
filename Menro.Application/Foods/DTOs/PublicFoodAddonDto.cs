using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.DTOs
{
    public class PublicFoodAddonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ExtraPrice { get; set; }
    }

}
