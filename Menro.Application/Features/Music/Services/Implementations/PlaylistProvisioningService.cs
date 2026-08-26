using System.Collections.Concurrent;
using Menro.Application.Features.Music.Services.Interfaces;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Music.Services.Implementations
{
    public class PlaylistProvisioningService : IPlaylistProvisioningService
    {
        private readonly IUnitOfWork _uow;

        // یک قفل جداگانه به‌ازای هر رستوران، تا ریکوئست‌های موازی برای همون رستوران
        // سریالایز بشن ولی رستوران‌های مختلف مانع هم نشن
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

        public PlaylistProvisioningService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Playlist> EnsureActivePlaylistAsync(int restaurantId)
        {
            var gate = _locks.GetOrAdd(restaurantId, _ => new SemaphoreSlim(1, 1));

            await gate.WaitAsync();
            try
            {
                var playlists = await _uow.Playlist.GetAllByRestaurantIdAsync(restaurantId);

                var active = playlists.FirstOrDefault(x => x.IsActive);
                if (active != null)
                    return active;

                Playlist target;

                if (playlists.Any())
                {
                    target = playlists.OrderBy(x => x.CreatedAt).First();
                }
                else
                {
                    target = new Playlist
                    {
                        Id = Guid.NewGuid(),
                        RestaurantId = restaurantId,
                        Name = "پلی‌لیست اصلی",
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _uow.Playlist.AddAsync(target);
                }

                foreach (var p in playlists)
                    p.IsActive = p.Id == target.Id;

                target.IsActive = true;

                await _uow.SaveChangesAsync();

                return target;
            }
            finally
            {
                gate.Release();
            }
        }
    }
}