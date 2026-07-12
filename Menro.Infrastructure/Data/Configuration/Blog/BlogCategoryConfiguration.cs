using Menro.Domain.Entities.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration
{
    public class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
    {
        public void Configure(EntityTypeBuilder<BlogCategory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Subtitle)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.ColorHex)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(x => x.SortOrder)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            // Not unique on purpose: two rows briefly share a SortOrder value
            // while a move-up/move-down swap is being persisted.
            builder.HasIndex(x => x.SortOrder);
        }
    }
}
