using Menro.Domain.Entities.Music;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Configuration
{
    public class PlaylistConfiguration
        : IEntityTypeConfiguration<Playlist>
    {
        public void Configure(
            EntityTypeBuilder<Playlist> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.HasMany(x => x.Tracks)
                .WithOne(x => x.Playlist)
                .HasForeignKey(x => x.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}