using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.DTOs
{
    public class RegisterRestaurantDto
    {
        [Required(ErrorMessage = "نام رستوران الزامی است")]
        [MaxLength(50)]
        public string RestaurantName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string RestaurantDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "آدرس رستوران الزامی است")]
        public string RestaurantAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "شناسه دسته‌بندی رستوران الزامی است")]
        public int RestaurantCategoryId { get; set; }

        [Required(ErrorMessage = "ساعت شروع فعالیت الزامی است")]
        public TimeSpan RestaurantOpenTime { get; set; }

        [Required(ErrorMessage = "ساعت پایان فعالیت الزامی است")]
        public TimeSpan RestaurantCloseTime { get; set; }

        [Required(ErrorMessage = "کد ملی الزامی است")]
        [MaxLength(10)]
        public string OwnerNationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره حساب الزامی است")]
        [MaxLength(15)]
        public string RestaurantAccountNumber { get; set; } = string.Empty;
    }
}
