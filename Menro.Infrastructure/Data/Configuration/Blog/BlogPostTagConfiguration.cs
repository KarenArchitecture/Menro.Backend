using Menro.Domain.Entities.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration
{
    public class BlogPostTagConfiguration : IEntityTypeConfiguration<BlogPostTag>
    {
        public void Configure(EntityTypeBuilder<BlogPostTag> builder)
        {
            builder.HasKey(x => x.Id);

            // A post can't be tagged with the same tag twice.
            builder.HasIndex(x => new { x.BlogPostId, x.BlogTagId })
                .IsUnique();

            builder.HasOne(x => x.BlogPost)
                .WithMany(x => x.PostTags)
                .HasForeignKey(x => x.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.BlogTag)
                .WithMany(x => x.PostTags)
                .HasForeignKey(x => x.BlogTagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
