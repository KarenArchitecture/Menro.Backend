using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Menro.Domain.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "نام کامل")]
        [MaxLength(50)]
        [Required(ErrorMessage = "نام کامل الزامی است")]
        public required string FullName { get; set; }

        [Display(Name = "آدرس عکس")]
        public string? ProfileImage { get; set; } = string.Empty;
        // تاریخ عضویت
        public ICollection<Restaurant>? Restaurants { get; set; } = new List<Restaurant>();
        // ارتباط با سفارشات مربوط به کاربر
        public ICollection<Order> Orders { get; set; } = new List<Order>(); // این رو اضافه کن

    }
}
