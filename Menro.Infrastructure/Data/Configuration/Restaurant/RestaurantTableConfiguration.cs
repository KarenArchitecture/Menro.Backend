using Menro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration
{
    public class RestaurantTableConfiguration
        : IEntityTypeConfiguration<RestaurantTable>
    {
        public void Configure(EntityTypeBuilder<RestaurantTable> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Label)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(x => x.Restaurant)
                .WithMany(x => x.Tables)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}