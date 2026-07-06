using Menro.Domain.Entities;
using Menro.Domain.Entities.Identity;
using Menro.Domain.Entities.Music;
using Menro.Infrastructure.Data.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data
{
    public class MenroDbContext : IdentityDbContext<User>
    {
        public MenroDbContext(DbContextOptions<MenroDbContext> options)
            : base(options)
        {
        }

        /* ===================== DBSets ===================== */

        public DbSet<User> Users { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<CustomFoodCategory> CustomFoodCategories { get; set; }
        public DbSet<GlobalFoodCategory> GlobalFoodCategories { get; set; }
        public DbSet<Icon> Icons { get; set; }
        public DbSet<FoodRating> FoodRatings { get; set; }
        public DbSet<FoodVariant> FoodVariants { get; set; }
        public DbSet<FoodAddon> FoodAddons { get; set; }
        public DbSet<FavoriteFood> FavoriteFoods { get; set; }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<RestaurantCategory> RestaurantCategories { get; set; }
        public DbSet<RestaurantRating> RestaurantRatings { get; set; }
        public DbSet<RestaurantAd> RestaurantAds { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<AdPricingSetting> AdPricingSettings { get; set; }

        public DbSet<Discount> Discounts { get; set; }

        public DbSet<Otp> Otps { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemExtra> OrderItemExtras { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /* ===================== MUSIC ===================== */

        public DbSet<MusicTrack> MusicTracks { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public DbSet<MusicPlayer> MusicPlayers { get; set; }
        public DbSet<TrackRequest> TrackRequests { get; set; }


        /* ===================== SAVE ===================== */

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
            => await base.SaveChangesAsync(cancellationToken);

        /* ===================== MODEL ===================== */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureConfigurations(modelBuilder);

            ApplySoftDeleteFilter(modelBuilder);
            ConfigureRelations(modelBuilder);
            ConfigureIndexes(modelBuilder);
            SeedStaticData(modelBuilder);
        }

        /* ===================== CONFIGURATIONS ===================== */

        private void ConfigureConfigurations(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(MenroDbContext).Assembly);
        }

        /* ===================== SOFT DELETE ===================== */

        private void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Food>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<Restaurant>().HasQueryFilter(x => !x.IsDeleted);

            modelBuilder.Entity<CustomFoodCategory>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<GlobalFoodCategory>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<FoodVariant>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<FoodAddon>().HasQueryFilter(x => !x.IsDeleted);

            modelBuilder.Entity<OrderItemExtra>()
                .HasQueryFilter(x => !x.OrderItem.Food.IsDeleted);

            modelBuilder.Entity<FoodRating>()
                .HasQueryFilter(x => !x.Food.IsDeleted);

            modelBuilder.Entity<FavoriteFood>()
                .HasQueryFilter(x => !x.Food.IsDeleted);

            modelBuilder.Entity<OrderItem>()
                .HasQueryFilter(x => !x.Food.IsDeleted);

            modelBuilder.Entity<RestaurantRating>()
                .HasQueryFilter(x => !x.Restaurant.IsDeleted);

            modelBuilder.Entity<RestaurantAd>()
                .HasQueryFilter(x => !x.Restaurant.IsDeleted);

            modelBuilder.Entity<Subscription>()
                .HasQueryFilter(x => !x.Restaurant.IsDeleted);

            modelBuilder.Entity<Order>()
                .HasQueryFilter(x => !x.Restaurant.IsDeleted);
        }

        /* ===================== RELATIONS ===================== */

        private void ConfigureRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Food>()
                .HasOne(f => f.CustomFoodCategory)
                .WithMany(c => c.Foods)
                .HasForeignKey(f => f.CustomFoodCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Food>()
                .HasOne(f => f.GlobalFoodCategory)
                .WithMany(g => g.Foods)
                .HasForeignKey(f => f.GlobalFoodCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Food>()
                .HasMany(f => f.Ratings)
                .WithOne(r => r.Food)
                .HasForeignKey(r => r.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FavoriteFood>()
                .HasOne(x => x.User)
                .WithMany(x => x.FavoriteFoods)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FavoriteFood>()
                .HasOne(x => x.Food)
                .WithMany(x => x.FavoriteFoods)
                .HasForeignKey(x => x.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Restaurant>()
                .HasMany(r => r.Ratings)
                .WithOne(r => r.Restaurant)
                .HasForeignKey(r => r.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Restaurant>()
                .HasMany(r => r.Advertisements)
                .WithOne(a => a.Restaurant)
                .HasForeignKey(a => a.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Food)
                .WithMany(f => f.OrderItems)
                .HasForeignKey(i => i.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Restaurant)
                .WithOne(r => r.Subscription)
                .HasForeignKey<Subscription>(s => s.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .Property(x => x.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(x => x.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Discount>()
                .Property(x => x.Value)
                .HasColumnType("decimal(18,2)");
        }

        /* ===================== INDEXES ===================== */

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GlobalFoodCategory>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<CustomFoodCategory>()
                .HasIndex(x => new { x.RestaurantId, x.Name })
                .IsUnique();

            modelBuilder.Entity<FavoriteFood>()
                .HasIndex(x => new { x.UserId, x.FoodId })
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(x => new { x.RestaurantId, x.RestaurantOrderNumber })
                .IsUnique();

            modelBuilder.Entity<RestaurantRating>()
                .HasIndex(x => new { x.UserId, x.RestaurantId })
                .IsUnique();

            modelBuilder.Entity<AdPricingSetting>()
                .HasIndex(x => new { x.PlacementType, x.BillingType })
                .IsUnique();

            modelBuilder.Entity<Restaurant>().HasIndex(x => x.IsActive);
            modelBuilder.Entity<Food>().HasIndex(x => x.RestaurantId);
            modelBuilder.Entity<Food>().HasIndex(x => x.GlobalFoodCategoryId);
            modelBuilder.Entity<Order>().HasIndex(x => x.UserId);
            modelBuilder.Entity<Order>().HasIndex(x => x.RestaurantId);
        }

        /* ===================== SEED ===================== */

        private void SeedStaticData(ModelBuilder modelBuilder)
        {
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