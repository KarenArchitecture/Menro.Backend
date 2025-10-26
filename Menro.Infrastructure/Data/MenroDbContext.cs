using Menro.Application.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
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
        public DbSet<CustomFoodCategory> CustomFoodCategories { get; set; }
        public DbSet<GlobalFoodCategory> GlobalFoodCategories { get; set; }
        public DbSet<FoodRating> FoodRatings { get; set; }
        public DbSet<FoodVariant> FoodVariants { get; set; }
        public DbSet<FoodAddon> FoodAddons { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<RestaurantCategory> RestaurantCategories { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<RestaurantDiscount> RestaurantDiscounts { get; set; }
        public DbSet<RestaurantRating> RestaurantRatings { get; set; }
        public DbSet<RestaurantAdBanner> RestaurantAdBanners { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* ---------------------------- Fluent API ---------------------------- */

            // Food -> CustomFoodCategory (optional)
            modelBuilder.Entity<Food>()
                .HasOne(f => f.CustomFoodCategory)
                .WithMany(c => c.Foods) // make sure CustomFoodCategory has ICollection<Food> Foods
                .HasForeignKey(f => f.CustomFoodCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Food -> GlobalFoodCategory (optional)
            modelBuilder.Entity<Food>()
                .HasOne(f => f.GlobalFoodCategory)
                .WithMany(g => g.Foods) // make sure GlobalFoodCategory has ICollection<Food> Foods
                .HasForeignKey(f => f.GlobalFoodCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Food -> GlobalFoodCategory (optional)
            modelBuilder.Entity<Food>()
                .HasOne(f => f.GlobalFoodCategory)
                .WithMany(g => g.Foods) // make sure GlobalFoodCategory has ICollection<Food> Foods
                .HasForeignKey(f => f.GlobalFoodCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // FoodCategory -> Restaurant (Many-to-One)
            modelBuilder.Entity<CustomFoodCategory>()
                .HasOne(fc => fc.Restaurant)
                .WithMany(r => r.FoodCategories)
                .HasForeignKey(fc => fc.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Food <-> FoodVariant (One-to-Many)
            modelBuilder.Entity<FoodVariant>()
                .HasOne(v => v.Food)
                .WithMany(f => f.Variants)
                .HasForeignKey(v => v.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            // FoodVariant <-> FoodAddon (One-to-Many)
            modelBuilder.Entity<FoodAddon>()
                .HasOne(a => a.FoodVariant)
                .WithMany(v => v.Addons)
                .HasForeignKey(a => a.FoodVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GlobalFoodCategory>()
                .HasIndex(gc => gc.Name)
                .IsUnique();

            modelBuilder.Entity<CustomFoodCategory>()
                .HasIndex(fc => new { fc.RestaurantId, fc.Name })
                .IsUnique();

            modelBuilder.Entity<CustomFoodCategory>()
                .HasOne(fc => fc.Restaurant)
                .WithMany(r => r.FoodCategories)
                .HasForeignKey(fc => fc.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restaurant <-> RestaurantCategory (Many-to-One)
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.RestaurantCategory)
                .WithMany(c => c.Restaurants)
                .HasForeignKey(r => r.RestaurantCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restaurant -> User (One-to-Many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Restaurants)
                .WithOne(r => r.OwnerUser)
                .HasForeignKey(r => r.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restaurant → AdBanner (One-to-One)
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.AdBanner)
                .WithOne(b => b.Restaurant)
                .HasForeignKey<RestaurantAdBanner>(b => b.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Subscription -> SubscriptionPlan (Many-to-One)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.SubscriptionPlan)
                .WithMany(sp => sp.Subscriptions)
                .HasForeignKey(s => s.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restaurant -> Subscription (One-to-One)
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.Subscription)
                .WithOne(s => s.Restaurant)
                .HasForeignKey<Subscription>(s => s.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // RestaurantRating: (User, Restaurant) pair is unique
            modelBuilder.Entity<RestaurantRating>()
                .HasIndex(r => new { r.UserId, r.RestaurantId })
                .IsUnique();

            // RestaurantDiscount -> optional Food
            modelBuilder.Entity<RestaurantDiscount>()
                .HasOne(d => d.Food)
                .WithMany()
                .HasForeignKey(d => d.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            /* for order module */
            // User → Order (One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                // مقدار پیش‌فرض برای Status
                entity.Property(o => o.Status)
                      .HasDefaultValue(OrderStatus.Pending);

                // مقدار پیش‌فرض برای CreatedAt
                entity.Property(o => o.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()"); // برای SQL Server
            });

            // Restaurant → Order (One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Restaurant)
                .WithMany(r => r.Orders)
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order → OrderItem (One-to-Many)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Food → OrderItem (One-to-Many)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Food)
                .WithMany(f => f.OrderItems)
                .HasForeignKey(oi => oi.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            // مقدار دقیق decimal برای قیمت
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            /* -------------------- Indexes for Home Page Features -------------------- */

            // ---- Carousel & Ad Banners ----
            modelBuilder.Entity<RestaurantAdBanner>()
                .HasIndex(b => b.RestaurantId);

            modelBuilder.Entity<Restaurant>()
                .HasIndex(r => r.IsActive);
            modelBuilder.Entity<Restaurant>()
                .HasIndex(r => r.IsApproved);

            // ---- Random Restaurant Cards ----
            modelBuilder.Entity<Restaurant>()
                .HasIndex(r => new { r.IsActive, r.IsApproved });

            // ---- Popular Foods / Home Food Cards ----
            modelBuilder.Entity<Food>()
                .HasIndex(f => f.GlobalFoodCategoryId);
            modelBuilder.Entity<Food>()
                .HasIndex(f => f.RestaurantId);

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.FoodId);

            modelBuilder.Entity<FoodRating>()
                .HasIndex(fr => fr.FoodId);

            // ---- Recent Orders ----
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId);
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.RestaurantId);

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId);
            /* ---------------------------- Seed Data ---------------------------- */
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