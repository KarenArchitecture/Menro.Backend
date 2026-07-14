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

            builder.Property(x => x.IsPublished)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            // Category is now a real FK to BlogCategory, not an enum.
            builder.Property(x => x.CategoryId)
                .IsRequired();

            builder.HasOne(x => x.Category)
                .WithMany() // change to .WithMany(c => c.Posts) if you add a Posts collection to BlogCategory
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CategoryId);
            builder.HasIndex(x => x.IsPublished);
        }
    }
}
