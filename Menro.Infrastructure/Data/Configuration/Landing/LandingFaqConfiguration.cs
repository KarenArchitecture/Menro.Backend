using Menro.Domain.Entities.Landing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration
{
    public class LandingFaqConfiguration : IEntityTypeConfiguration<LandingFaq>
    {
        public void Configure(EntityTypeBuilder<LandingFaq> builder)
        {
            builder.ToTable("LandingFaqs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Question)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(x => x.Answer)
                .IsRequired()
                .HasMaxLength(1200);

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
