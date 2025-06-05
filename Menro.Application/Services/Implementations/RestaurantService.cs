using Menro.Application.DTO;
using Menro.Application.Services.Interfaces;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Implementations
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
                OwnerFullName = dto.OwnerFullName,
                NationalCode = dto.NationalCode,
                PhoneNumber = dto.PhoneNumber,
                BankAccountNumber = dto.BankAccountNumber,
                ShebaNumber = dto.ShebaNumber,
                RestaurantCategoryId = dto.RestaurantCategoryId
            };
            
            return await _uow.Restaurant.AddAsync(restaurant);
        }
    }
}
