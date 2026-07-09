using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Restaurants.DTOs
{
    public class CreateRestaurantCategoryDto
    {
        [Required(ErrorMessage = "نام دسته‌بندی الزامی است")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateRestaurantCategoryDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "نام دسته‌بندی الزامی است")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
