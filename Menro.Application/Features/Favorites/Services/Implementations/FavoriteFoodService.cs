using Menro.Application.Features.Favorites.DTOs;
using Menro.Application.Features.Favorites.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Favorites.Services.Implementations
{
    public class FavoriteFoodService : IFavoriteFoodService
    {
        private readonly IFavoriteFoodRepository _repo;
        private readonly IFoodRepository _foodRepo;

        public FavoriteFoodService(
            IFavoriteFoodRepository repo,
            IFoodRepository foodRepo)
        {
            _repo = repo;
            _foodRepo = foodRepo;
        }

        public async Task ToggleAsync(string userId, int foodId)
        {
            var exists = await _repo.ExistsAsync(userId, foodId);

            if (exists)
            {
                var entity = await _repo.GetAsync(userId, foodId);
                if (entity != null)
                    await _repo.RemoveAsync(entity);
            }
            else
            {
                await _repo.AddAsync(new FavoriteFood
                {
                    UserId = userId,
                    FoodId = foodId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        public async Task<List<FavoriteFoodDto>> GetUserFavoritesAsync(string userId)
        {
            var foodIds = await _repo.GetFavoriteFoodIdsByUserAsync(userId);

            if (!foodIds.Any())
                return new List<FavoriteFoodDto>();

            var foods = await _foodRepo.GetFoodsByIdsAsync(foodIds);

            return foods.Select(f => new FavoriteFoodDto
            {
                FoodId = f.Id,
                FoodName = f.Name,
                RestaurantName = f.Restaurant.Name,
                ImageUrl = f.ImageUrl
            }).ToList();
        }

        public async Task<List<int>> GetFavoriteFoodIdsAsync(string userId)
        {
            return await _repo.GetFavoriteFoodIdsByUserAsync(userId);
        }
    }
}
