// Application/Features/Restaurants/Services/Implementations/RestaurantPaymentSettingsService.cs
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RestaurantPaymentSettingsService : IRestaurantPaymentSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RestaurantPaymentSettingsService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<RestaurantPaymentMethodDto> GetAsync(int restaurantId)
        {
            var restaurant = await _unitOfWork.Restaurant.GetByIdAsync(restaurantId)
                ?? throw new Exception("رستوران یافت نشد.");

            return new RestaurantPaymentMethodDto { PaymentMethod = restaurant.PaymentMethod };
        }

        public async Task SetAsync(int restaurantId, UpdateRestaurantPaymentMethodDto dto)
        {
            if (dto.PaymentMethod == RestaurantPaymentMethod.BankGateway)
                throw new Exception("درگاه بانکی هنوز فعال نشده است.");

            var restaurant = await _unitOfWork.Restaurant.GetByIdAsync(restaurantId)
                ?? throw new Exception("رستوران یافت نشد.");

            restaurant.PaymentMethod = dto.PaymentMethod;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}