using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class FoodCategory
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "نام دسته بندی")]
        [MaxLength(200)]
        [Required(ErrorMessage = "نام دسته بندی الزامی است")]
        public string Name { get; set; } // مثل "نوشیدنی سرد"، "پیتزا"

        // ارتباط با جدول رستوران
        public int RestaurantId { get; set; }

        public Restaurant Restaurant { get; set; } = null!;

        public ICollection<Food> Foods { get; set; } = new List<Food>();
    }
}
