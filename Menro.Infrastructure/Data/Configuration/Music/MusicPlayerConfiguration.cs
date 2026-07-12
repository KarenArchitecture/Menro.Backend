using Menro.Domain.Entities.Music;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Configuration
{
    public class MusicPlayerConfiguration : IEntityTypeConfiguration<MusicPlayer>
    {
        public void Configure(EntityTypeBuilder<MusicPlayer> builder)
        {
            builder.HasKey(x => x.RestaurantId);

            builder.Property(x => x.RestaurantId)
                .ValueGeneratedNever();
        }
    }
}
