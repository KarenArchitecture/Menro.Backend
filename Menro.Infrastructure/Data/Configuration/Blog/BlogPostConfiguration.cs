using Menro.Domain.Entities.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration
{
    public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
    {
        public void Configure(EntityTypeBuilder<BlogPost> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.CoverImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.ReadingMinutes)
                .IsRequired();

            builder.Property(x => x.Category)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.IsPublished)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => x.Category);
            builder.HasIndex(x => x.IsPublished);
        }
    }
}
