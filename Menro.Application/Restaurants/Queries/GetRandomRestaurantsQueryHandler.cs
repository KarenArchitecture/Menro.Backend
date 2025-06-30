using MediatR;
using Menro.Application.Restaurants.DTOs;
using Menro.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Queries
{
    public class GetRandomRestaurantsQueryHandler : IRequestHandler<GetRandomRestaurantsQuery, List<RestaurantDto>>
    {
        private readonly IMenroDbContext _db;

        public GetRandomRestaurantsQueryHandler(IMenroDbContext db)
        {
            _db = db;
        }

        public async Task<List<RestaurantDto>> Handle(GetRandomRestaurantsQuery request, CancellationToken cancellationToken)
        {
            var randomRestaurants = await _db.Restaurants
                .Include(r => r.RestaurantCategory)
                .OrderBy(r => Guid.NewGuid()) // random order
                .Take(request.Count)
                .Select(r => new RestaurantDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    BannerImageUrl = r.BannerImageUrl ?? "",
                    CategoryName = r.RestaurantCategory.Name,
                    OpenTime = r.OpenTime,
                    CloseTime = r.CloseTime,
                    // Dummy data for now; you'll add real logic later
                    Rating = 4.2, // Optional: calculate average from Votes
                    Voters = 127, // Optional: count votes
                    Discount = null // Optional: fill this from restaurant-wide offer later
                })
                .ToListAsync(cancellationToken);

            return randomRestaurants;
        }
    }
}