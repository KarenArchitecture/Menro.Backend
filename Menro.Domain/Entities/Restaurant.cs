using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Domain.Entities
{
    public class Restaurant
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام رستوران الزامی است")]
        public string Name { get; set; } = string.Empty;

        public string? BannerImageUrl { get; set; }

        [Required(ErrorMessage = "افزودن آدرس رستوران الزامی است")]
        public string Address { get; set; } = string.Empty;

        // مشخصات صاحب رستوران
        [Required(ErrorMessage = "نام صاحب رستوران الزامی است")]
        public string OwnerFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد ملی صاحب رستوران الزامی است")]
        public string NationalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره حساب صاحب رستوران الزامی است")]
        public string BankAccountNumber { get; set; } = string.Empty;

        public string? ShebaNumber { get; set; }
        // اتصال رستوران به صاحب رستوران
        public string OwnerUserId { get; set; } = string.Empty;
        public User OwnerUser { get; set; } = null!;
        // اشتراک
        public Subscription? Subscription { get; set; }

        // دسته بندی رستوران (Navigation property + FK)
        public int RestaurantCategoryId { get; set; }

        public RestaurantCategory RestaurantCategory { get; set; } = null!;

        // غذاها
        public ICollection<Food> Foods { get; set; } = new List<Food>();
        public ICollection<FoodCategory> FoodCategories { get; set; } = new List<FoodCategory>();
    }
}
