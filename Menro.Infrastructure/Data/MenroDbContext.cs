using Menro.Domain.Entities;
using Menro.Domain.Entities.Blog;
using Menro.Domain.Entities.Identity;
using Menro.Domain.Entities.Music;
using Menro.Domain.Entities.SiteContent;
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
        public DbSet<FoodCombo> FoodCombos { get; set; }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<RestaurantTable> RestaurantTables { get; set; }
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

        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<CartItemExtra> CartItemExtras => Set<CartItemExtra>();

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }

        /* ===================== MUSIC ===================== */

        public DbSet<MusicTrack> MusicTracks { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public DbSet<MusicPlayer> MusicPlayers { get; set; }
        public DbSet<TrackRequest> TrackRequests { get; set; }

        /* ===================== BLOG ===================== */

        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<BlogHero> BlogHeroes { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogPostContent> BlogPostContents { get; set; }
        public DbSet<BlogPostTag> BlogPostTags { get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }
        public DbSet<BlogPostLike> BlogPostLikes { get; set; }

        /* ===================== SITE CONFIG ===================== */
        // landing
        public DbSet<LandingGeneral> LandingGeneral { get; set; }
        public DbSet<LandingFaq> LandingFaqs { get; set; }
        public DbSet<LandingReason> LandingReasons { get; set; }

        // common
        public DbSet<MenuItem> MenuItems { get; set; }

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
            modelBuilder.Entity<RestaurantTable>().HasQueryFilter(x => !x.IsDeleted);

            modelBuilder.Entity<CustomFoodCategory>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<GlobalFoodCategory>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<FoodVariant>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<FoodAddon>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<FoodCombo>()
                .HasQueryFilter(x => !x.Food.IsDeleted && !x.ComboFood.IsDeleted);
            modelBuilder.Entity<Comment>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<CommentLike>()
                .HasQueryFilter(x => !x.Comment.IsDeleted);

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

            modelBuilder.Entity<FoodCombo>()
                .HasOne(fc => fc.Food)
                .WithMany()
                .HasForeignKey(fc => fc.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FoodCombo>()
                .HasOne(fc => fc.ComboFood)
                .WithMany()
                .HasForeignKey(fc => fc.ComboFoodId)
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
                .HasMany(r => r.Advertisements)
                .WithOne(a => a.Restaurant)
                .HasForeignKey(a => a.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Restaurant>()
                .HasIndex(x => x.OwnerUserId)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0 AND [OwnerUserId] IS NOT NULL");

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Food)
                .WithMany()
                .HasForeignKey(ci => ci.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.FoodVariant)
                .WithMany()
                .HasForeignKey(ci => ci.FoodVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItemExtra>()
                .HasOne(cie => cie.CartItem)
                .WithMany(ci => ci.Extras)
                .HasForeignKey(cie => cie.CartItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItemExtra>()
                .HasOne(cie => cie.FoodAddon)
                .WithMany()
                .HasForeignKey(cie => cie.FoodAddonId)
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

            modelBuilder.Entity<Discount>()
                .Property(x => x.Value)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Restaurant)
                .WithOne(r => r.Subscription)
                .HasForeignKey<Subscription>(s => s.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Food)
                .WithMany()
                .HasForeignKey(c => c.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommentLike>()
                .HasOne(l => l.Comment)
                .WithMany(c => c.Likes)
                .HasForeignKey(l => l.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommentLike>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<FoodCombo>()
                .HasIndex(x => new { x.FoodId, x.ComboFoodId })
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

            modelBuilder.Entity<Comment>().HasIndex(x => x.FoodId);
            modelBuilder.Entity<Comment>().HasIndex(x => x.Status);
            modelBuilder.Entity<CommentLike>()
                .HasIndex(x => new { x.CommentId, x.UserId, x.Target })
                .IsUnique();
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