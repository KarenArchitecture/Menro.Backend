using Menro.Domain.Entities;
using Menro.Infrastructure.Migrations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data
{
    public class MenroDbContext : IdentityDbContext<User>
    {
        public MenroDbContext(DbContextOptions<MenroDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<FoodCategory> FoodCategories { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<RestaurantCategory> RestaurantCategories { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            /* ---------------------------- Fluent API ---------------------------- */
            // Many-to-Many: Food <-> FoodCategory
            modelBuilder.Entity<Food>()
                    .HasOne(f => f.Category)
                    .WithMany(c => c.Foods)
                    .HasForeignKey(f => f.FoodCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

            // One-to-Many: FoodCategory -> Restaurant
            modelBuilder.Entity<FoodCategory>()
                .HasOne(fc => fc.Restaurant)
                .WithMany(r => r.FoodCategories)
                .HasForeignKey(fc => fc.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many: Restaurant <-> RestaurantCategory
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.RestaurantCategory)
                .WithMany(c => c.Restaurants)
                .HasForeignKey(r => r.RestaurantCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-One: Restaurant -> User
            modelBuilder.Entity<User>()
                .HasMany(u => u.Restaurants)
                      .WithOne(r => r.OwnerUser)
                      .HasForeignKey(r => r.OwnerUserId)
                      .OnDelete(DeleteBehavior.Restrict);

            // One-to-Many: Subscription -> SubscriptionPlan
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.SubscriptionPlan)
                .WithMany(sp => sp.Subscriptions)
                .HasForeignKey(s => s.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-One: Restaurant -> Subscription
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.Subscription)
                .WithOne(s => s.Restaurant)
                .HasForeignKey<Subscription>(s => s.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade); // یا Restrict برحسب نیاز

            /* ---------------------------- Seed Data ---------------------------- */
            // Seed داده‌های دسته‌بندی رستوران‌ها
            modelBuilder.Entity<RestaurantCategory>().HasData(
                new RestaurantCategory { Id = 1, Name = "رستوران سنتی" },
                new RestaurantCategory { Id = 2, Name = "رستوران مدرن" },
                new RestaurantCategory { Id = 3, Name = "چینی/آسیایی" },
                new RestaurantCategory { Id = 4, Name = "ایتالیایی" },
                new RestaurantCategory { Id = 5, Name = "کافه رستوران" },
                new RestaurantCategory { Id = 6, Name = "فست‌فود" },
                new RestaurantCategory { Id = 7, Name = "باغ رستوران" },
                new RestaurantCategory { Id = 8, Name = "دریایی" }
            );

        }
    }
}
