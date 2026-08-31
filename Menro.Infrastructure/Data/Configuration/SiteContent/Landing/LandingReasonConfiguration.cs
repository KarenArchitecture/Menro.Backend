using Menro.Domain.Entities.SiteContent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration.SiteContent
{
    public class LandingReasonConfiguration : IEntityTypeConfiguration<LandingReason>
    {
        public void Configure(EntityTypeBuilder<LandingReason> builder)
        {
            builder.ToTable("LandingReasons");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Icon)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ColorHex)
                .IsRequired()
                .HasMaxLength(9); // "#RRGGBBAA" worst case

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.SortOrder)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => x.SortOrder);
        }
    }
}
