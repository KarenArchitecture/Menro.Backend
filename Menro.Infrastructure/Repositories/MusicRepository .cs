using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Menro.Infrastructure.Repositories
{
    public class MusicRepository : IMusicRepository
    {
        private readonly MenroDbContext _context;

        public MusicRepository(MenroDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Music music)
        {
            await _context.Musics.AddAsync(music);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Music music)
        {
            _context.Musics.Update(music);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Music music)
        {
            _context.Musics.Remove(music);
            await _context.SaveChangesAsync();
        }

        public async Task<Music?> GetByIdAsync(Guid id)
        {
            return await _context.Musics.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Music>> GetAllAsync()
        {
            return await _context.Musics
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Music>> SearchAsync(string searchTerm)
        {
            return await _context.Musics
                .Where(x =>
                    x.Title.Contains(searchTerm) ||
                    x.Artist.Contains(searchTerm))
                .ToListAsync();
        }
    }
}
