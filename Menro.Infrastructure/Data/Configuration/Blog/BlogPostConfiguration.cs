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
            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
            builder.Property(x => x.ReadingMinutes).IsRequired();
            builder.Property(x => x.IsPublished).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.CreatedAtUtc).IsRequired();

            builder.Property(x => x.CategoryId).IsRequired(false);
            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.Slug).IsUnique();
            builder.HasIndex(x => x.CategoryId);
            builder.HasIndex(x => x.IsPublished);

            // for blog post author
            builder.Property(x => x.AuthorId)
                .IsRequired(false)
                .HasMaxLength(450);

            builder.Property(x => x.AuthorNameSnapshot)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(x => x.Author)
                .WithMany()
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.AuthorId);
        }
    }
}