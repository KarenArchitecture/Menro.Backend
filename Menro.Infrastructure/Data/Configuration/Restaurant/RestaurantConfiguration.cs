using Menro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration
{
    public class RestaurantConfiguration
        : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.Slug)
                .IsUnique();

            builder.Property(x => x.Address)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.NationalCode)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.BankAccountNumber)
                .HasMaxLength(34);

            builder.Property(x => x.ShebaNumber)
                .HasMaxLength(30);

            builder.Property(x => x.RejectReason)
                .HasMaxLength(500);

            builder.HasOne(x => x.OwnerUser)
                .WithMany()
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RestaurantCategory)
                .WithMany()
                .HasForeignKey(x => x.RestaurantCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}