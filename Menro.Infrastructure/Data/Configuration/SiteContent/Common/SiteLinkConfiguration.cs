using Menro.Domain.Entities.SiteContent;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Configuration.SiteContent
{
    public class SiteLinkConfiguration : IEntityTypeConfiguration<SiteLink>
    {
        public void Configure(EntityTypeBuilder<SiteLink> builder)
        {
            builder.ToTable("SiteLinks");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Url).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Location).IsRequired();
            builder.HasIndex(x => new { x.Location, x.Order });
        }
    }
}
