using Menro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Interfaces
{
    public interface IMenroDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Food> Foods { get; set; }
        DbSet<FoodCategory> FoodCategories { get; set; }
        DbSet<Restaurant> Restaurants { get; set; }
        DbSet<RestaurantCategory> RestaurantCategories { get; set; }
        DbSet<Subscription> Subscriptions { get; set; }
        DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        DbSet<RestaurantDiscount> RestaurantDiscounts { get; set; }
        DbSet<RestaurantRating> RestaurantRatings { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
