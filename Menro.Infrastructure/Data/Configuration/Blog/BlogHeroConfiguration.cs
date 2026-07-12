using Menro.Domain.Entities.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration
{
    public class BlogHeroConfiguration : IEntityTypeConfiguration<BlogHero>
    {
        public void Configure(EntityTypeBuilder<BlogHero> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TitleLine)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Highlight)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.SearchPlaceholder)
                .IsRequired()
                .HasMaxLength(150);

            // Singleton by convention (enforced in BlogHeroService/Repository,
            // not at the DB level) - there is only ever one row in this table.
        }
    }
}
