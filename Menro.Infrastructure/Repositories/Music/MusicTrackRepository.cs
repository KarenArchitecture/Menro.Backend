using Microsoft.EntityFrameworkCore;
using Menro.Domain.Entities.Music;
using Menro.Infrastructure.Data;
using Menro.Domain.Interfaces.Music;

namespace Menro.Infrastructure.Repositories.Music
{
    internal class MusicTrackRepository : IMusicTrackRepository
    {
        private readonly MenroDbContext _context;

        public MusicTrackRepository(MenroDbContext context)
        {
            _context = context;
        }

        // add music
        public async Task AddAsync(MusicTrack track)
        {
            await _context.MusicTracks.AddAsync(track);
        }

        // get music
        public async Task<MusicTrack?> GetByIdAsync(Guid id, int restaurantId)
        {
            return await _context.MusicTracks.FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == restaurantId);
        }

        // get musics
        public async Task<List<MusicTrack>> GetAllByRestaurantIdAsync(int restaurantId)
        {
            return await _context.MusicTracks
                .Where(x => x.RestaurantId == restaurantId)
                .OrderBy(x => x.Title)
                .ToListAsync();
        }


        // remove music
        public bool Remove(MusicTrack track)
        {
            try
            {
                _context.MusicTracks.Remove(track);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
        /*------------------------------------------------------------------------------*/
        /*------------------------------------------------------------------------------*/
        /*------------------------------------------------------------------------------*/


        public Task UpdateAsync(MusicTrack track)
        {
            _context.MusicTracks.Update(track);

            return Task.CompletedTask;
        }
    }
}
