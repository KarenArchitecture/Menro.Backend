using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.DTOs
{
    public class FoodsListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }   // فقط زمانی پر میشه که Variants نداشته باشه
        public bool? IsAvailable { get; set; } = true;  // Shows if food can be ordered
        // NEW: display name depending on which category is present
        public string FoodCategoryName { get; set; } = string.Empty;
        // optional: type of category for admin reference
        public string FoodCategoryType { get; set; } = string.Empty; // "custom" | "global"
    }
}
