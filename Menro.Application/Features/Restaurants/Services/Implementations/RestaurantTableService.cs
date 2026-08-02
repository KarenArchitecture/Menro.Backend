using Menro.Application.Common.Models;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.DTOs.RestaurantTables;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RestaurantTableService : IRestaurantTableService
    {
        private readonly IRestaurantTableRepository _repository;

        public RestaurantTableService(IRestaurantTableRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RestaurantTablesDto>> GetAllByRestaurantIdAsync(int restaurantId)
        {
            var tables = await _repository.GetAllByRestaurantIdAsync(restaurantId);

            return tables.Select(t => new RestaurantTablesDto
            {
                Id = t.Id,
                Label = t.Label
            }).ToList();
        }

        public async Task<Result> AddTableAsync(CreateRestaurantTableDto dto, int restaurantId)
        {
            if (dto is null || restaurantId == 0)
                return Result.Failure("اطلاعات ارسالی نامعتبر است.", ErrorCode.Invalid);

            var label = (dto.Label ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(label))
                return Result.Failure("برچسب میز الزامی است.", ErrorCode.Invalid);

            var table = new RestaurantTable
            {
                Label = label,
                RestaurantId = restaurantId
            };

            var added = await _repository.AddAsync(table);
            if (!added)
                return Result.Failure("افزودن میز موفق نبود.", ErrorCode.Failure);

            await _repository.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> UpdateTableAsync(UpdateRestaurantTableDto dto)
        {
            var table = await _repository.GetByIdAsync(dto.Id);
            if (table == null)
                return Result.Failure("میز موردنظر یافت نشد.", ErrorCode.NotFound);

            var label = (dto.Label ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(label))
                return Result.Failure("برچسب میز الزامی است.", ErrorCode.Invalid);

            table.Label = label;

            var updated = await _repository.UpdateAsync(table);
            if (!updated)
                return Result.Failure("ذخیره تغییرات موفق نبود.", ErrorCode.Failure);

            await _repository.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<bool> DeleteTableAsync(int tableId)
        {
            var table = await _repository.GetByIdAsync(tableId);
            if (table == null)
                return false;

            // soft delete — حذف فیزیکی نیست، چون ممکنه توی سفارش‌های قبلی رفرنس داشته باشه
            table.IsDeleted = true;

            var updated = await _repository.UpdateAsync(table);
            if (!updated)
                return false;

            await _repository.SaveChangesAsync();
            return true;
        }
    }
}