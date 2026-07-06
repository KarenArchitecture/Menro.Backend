using Menro.Domain.Entities.Music;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menro.Infrastructure.Data.Configuration
{
    public class TrackRequestConfiguration
        : IEntityTypeConfiguration<TrackRequest>
    {
        public void Configure(EntityTypeBuilder<TrackRequest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.RequestedAt)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.MusicTrackId)
                .IsRequired();

            builder.HasOne(x => x.MusicTrack)
                .WithMany()
                .HasForeignKey(x => x.MusicTrackId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PlaylistTrack>()
                .WithMany()
                .HasForeignKey(x => x.PlaylistTrackId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new { x.RestaurantId, x.UserId, x.MusicTrackId });

            builder.HasIndex(x => x.PlaylistTrackId);
        }
    }
}