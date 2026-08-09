using Menro.Domain.Entities.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration
{
    public class BlogPostContentConfiguration : IEntityTypeConfiguration<BlogPostContent>
    {
        public void Configure(EntityTypeBuilder<BlogPostContent> builder)
        {
            builder.HasKey(x => x.BlogPostId);

            builder.Property(x => x.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.BlogPost)
                .WithOne(p => p.Content)
                .HasForeignKey<BlogPostContent>(x => x.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}