using Menro.Domain.Entities.SiteContent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration.SiteContent
{
    public class LandingGeneralConfiguration : IEntityTypeConfiguration<LandingGeneral>
    {
        public void Configure(EntityTypeBuilder<LandingGeneral> builder)
        {
            builder.ToTable("LandingGeneral");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.HeroImageFileName)
                .HasMaxLength(255);

            builder.Property(x => x.HeroHighlight)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(x => x.HeroTitle)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(x => x.SpotlightTitle)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            // Seed the single settings row so GET /api/admin/landing/general
            // always has something to return, even before an admin ever saves.
            // (LandingGeneralRepository.GetOrCreateAsync() is a second line of
            // defense in case this migration hasn't run yet in some environment.)
            builder.HasData(new LandingGeneral
            {
                Id = LandingGeneral.SingletonId,
                HeroImageFileName = null,
                HeroHighlight = "منرو",
                HeroTitle = "بهترین همیار رستوران تو",
                SpotlightTitle = "با منرو تو چشم باش",
                UpdatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });
        }
    }
}
