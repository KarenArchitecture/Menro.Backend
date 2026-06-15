using Menro.Domain.Entities.Music;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Configuration
{
    public class PlaylistTrackConfiguration
        : IEntityTypeConfiguration<PlaylistTrack>
    {
        public void Configure(
            EntityTypeBuilder<PlaylistTrack> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SortOrder)
                .IsRequired();

            builder.HasOne(x => x.Playlist)
                .WithMany(x => x.Tracks)
                .HasForeignKey(x => x.PlaylistId);

            builder.HasOne(x => x.MusicTrack)
                .WithMany()
                .HasForeignKey(x => x.MusicTrackId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}