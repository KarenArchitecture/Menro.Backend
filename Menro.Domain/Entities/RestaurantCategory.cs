using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities
{
    public class RestaurantCategory
    {
        [Key]
        public int Id { get; set; }


        [Display(Name = "نوع رستوران")]
        [MaxLength(50)]
        [Required(ErrorMessage = "نوع رستوران الزامی است")]
        public string Name { get; set; } // مثل "کافه"، "فست‌فودی"

        // Navigation
        public ICollection<Restaurant> Restaurants { get; set; }

    }
}
