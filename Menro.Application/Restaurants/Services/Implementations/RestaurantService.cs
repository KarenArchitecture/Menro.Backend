using Menro.Application.DTO;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menro.Application.Restaurants.DTOs;
using Menro.Application.Restaurants.Services.Interfaces;

namespace Menro.Application.Restaurants.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IUnitOfWork _uow;
        public RestaurantService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<bool> AddRestaurantAsync(RestaurantDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var restaurant = new Restaurant
            {
                Name = dto.Name,
                BannerImageUrl = dto.BannerImageUrl,
                Address = dto.Address,
                NationalCode = dto.NationalCode,
                BankAccountNumber = dto.BankAccountNumber,
                ShebaNumber = dto.ShebaNumber,
                RestaurantCategoryId = dto.RestaurantCategoryId
            };

            var success = await _uow.Restaurant.AddAsync(restaurant);

            if (!success)
                return false;

            await _uow.SaveAsync(); // ✅ Commit to database here

            return true;
        }
    }
}
