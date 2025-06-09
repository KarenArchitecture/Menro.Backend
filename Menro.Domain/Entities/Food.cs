using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class Food
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "نام آیتم")]
        [MaxLength(200)]
        [Required(ErrorMessage = "نام آیتم الزامی است")]
        public string Name { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "قیمت")]
        [MaxLength(10)]
        [Required(ErrorMessage = "قیمت الزامی است")]
        public int Price { get; set; }

        [Display(Name = "آدرس عکس")]
        public string ImageUrl { get; set; }

        // ارتباط با رستوران
        public int RestaurantId { get; set; }

        public Restaurant Restaurant { get; set; } = null!;

        // ارتباط با دسته بندی غذاها
        public int FoodCategoryId { get; set; }

        public FoodCategory Category { get; set; } = null!;
    }
}
