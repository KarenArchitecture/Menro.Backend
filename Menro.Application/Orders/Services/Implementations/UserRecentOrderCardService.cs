// Menro.Application/Orders/Services/Implementations/UserRecentOrderCardService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Menro.Application.Orders.DTOs;
using Menro.Application.Orders.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;

namespace Menro.Application.Orders.Services.Implementations
{
    public class UserRecentOrderCardService : IUserRecentOrderCardService
    {
        private readonly IOrderRepository _orderRepository;

        public UserRecentOrderCardService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        private static RecentOrdersFoodCardDto Map(Food f)
        {
            // compute avg once, null/empty safe
            var ratings = f.Ratings ?? new List<FoodRating>(0);
            var avg = ratings.Count == 0 ? 0.0 : ratings.Average(r => r.Score);

            return new RecentOrdersFoodCardDto
            {
                Id = f.Id,
                Name = f.Name,
                ImageUrl = f.ImageUrl ?? string.Empty,
                Rating = Math.Round(avg, 1),
                Voters = ratings.Count,

                RestaurantId = f.RestaurantId,
                RestaurantName = f.Restaurant?.Name ?? string.Empty,
                RestaurantSlug = f.Restaurant?.Slug
            };
        }

        public async Task<List<RecentOrdersFoodCardDto>> GetUserRecentOrderedFoodsAsync(string userId, int count = 8)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<RecentOrdersFoodCardDto>();

            // small safety clamp
            if (count <= 0) count = 8;
            if (count > 32) count = 32;

            var foods = await _orderRepository.GetUserRecentlyOrderedFoodsAsync(userId, count);
            if (foods == null || foods.Count == 0)
                return new List<RecentOrdersFoodCardDto>();

            // repo already returns in most-recent-first order, keep it as-is
            return foods.Select(Map).ToList();
        }
    }
}
