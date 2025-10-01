using System.ComponentModel.DataAnnotations;


namespace Menro.Domain.Entities
{
    public class CustomFoodCategory
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "نام دسته بندی")]
        [MaxLength(200)]
        [Required(ErrorMessage = "نام دسته بندی الزامی است")]
        public string Name { get; set; } = string.Empty; // مثل "نوشیدنی سرد"، "پیتزا"

        [Display(Name = "آیکون SVG")]
        public string SvgIcon { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // ارتباط با جدول رستوران
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        // ارتباط با جدول غذا
        public ICollection<Food> Foods { get; set; } = new List<Food>();
    }
}
