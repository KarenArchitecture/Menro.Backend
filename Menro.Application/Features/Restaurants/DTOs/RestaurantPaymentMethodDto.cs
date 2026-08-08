// Application/Features/Restaurants/DTOs/RestaurantPaymentMethodDto.cs
using Menro.Domain.Enums;

namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantPaymentMethodDto
    {
        public RestaurantPaymentMethod PaymentMethod { get; set; }
    }

    public class UpdateRestaurantPaymentMethodDto
    {
        public RestaurantPaymentMethod PaymentMethod { get; set; }
    }
}