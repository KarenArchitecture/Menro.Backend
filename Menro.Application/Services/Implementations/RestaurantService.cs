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
                NationalCode = dto.NationalCode,
                BankAccountNumber = dto.BankAccountNumber,
                ShebaNumber = dto.ShebaNumber,
                RestaurantCategoryId = dto.RestaurantCategoryId
            };

            var success = await _uow.Restaurant.AddAsync(restaurant);

            if (!success)
                return false;

            await _uow.SaveChangesAsync(); // ✅ Commit to database here

            return true;
        }

        public async Task<IEnumerable<FeaturedRestaurantDto>> GetFeaturedRestaurantsAsync()
        {
            var featuredRestaurants = await _uow.Restaurant.GetAllAsync(r => r.IsFeatured == true);

            // Map the results to our new, lean DTO
            var dtos = featuredRestaurants.Select(r => new FeaturedRestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                CarouselImageUrl = r.CarouselImageUrl
            });

            return dtos;
        }
    }
}
