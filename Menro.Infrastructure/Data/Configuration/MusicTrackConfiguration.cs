using Menro.Domain.Entities.Music;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Configuration
{
    public class MusicTrackConfiguration
        : IEntityTypeConfiguration<MusicTrack>
    {
        public void Configure(
            EntityTypeBuilder<MusicTrack> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Artist)
                .HasMaxLength(200);

            builder.Property(x => x.AudioFileName)
                .IsRequired();
        }
    }
}
