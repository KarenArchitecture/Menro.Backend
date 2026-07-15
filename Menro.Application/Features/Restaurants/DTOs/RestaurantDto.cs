using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantDto
    {
        [Required(ErrorMessage = "نام رستوران الزامی است")]
        public string Name { get; set; } = string.Empty;

        public string? BannerImageUrl { get; set; }

        [Required(ErrorMessage = "افزودن آدرس رستوران الزامی است")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام صاحب رستوران الزامی است")]
        public string OwnerFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد ملی صاحب رستوران الزامی است")]
        public string NationalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره حساب صاحب رستوران الزامی است")]
        public string BankAccountNumber { get; set; } = string.Empty;

        public string? ShebaNumber { get; set; }

        // فقط کلید خارجی
        [Required(ErrorMessage = "دسته بندی رستوران الزامی است")]
        public int RestaurantCategoryId { get; set; }
    }
}
