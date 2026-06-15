using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Music.DTOs;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Music.Services
{
    internal class MusicTrackService : IMusicTrackService
    {
        private readonly IUnitOfWork _uow;

        public MusicTrackService( IUnitOfWork uow)
        {
            _uow = uow;
        }


        // add music
        public async Task<MusicTrack> CreateAsync(int restaurantId, CreateMusicTrackDto dto)
        {
            var track = new MusicTrack
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,

                Title = dto.Title,
                Artist = dto.Artist,
                Duration = dto.Duration,

                AudioFileName = dto.AudioFileName,
                CoverFileName = dto.CoverFileName,

            };

            await _uow.MusicTrack.AddAsync(track);
            await _uow.SaveChangesAsync();

            return track;
        }

        // get musics
        public async Task<List<MusicTrackListItemDto>> GetAllAsync(int restaurantId)
        {
            var tracks = await _uow.MusicTrack.GetAllByRestaurantIdAsync(restaurantId);

            return tracks.Select(t => new MusicTrackListItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Artist = t.Artist,
                Duration = t.Duration,
                CoverFileName = t.CoverFileName,
            }).ToList();
        }


        // get music
        public async Task<MusicTrackDto?> GetByIdAsync(Guid trackId, int restaurantId)
        {
            var track = await _uow.MusicTrack.GetByIdAsync(trackId, restaurantId);

            if (track == null)
                return null;

            return new MusicTrackDto
            {
                Id = track.Id,
                Title = track.Title,
                Artist = track.Artist,
                Duration = track.Duration,

                AudioUrl = track.AudioFileName,
                CoverUrl = track.CoverFileName,
            };
        }

        // remove music
        public async Task<MusicTrack?> RemoveAsync(Guid trackId,int restaurantId)
        {
            var track = await _uow.MusicTrack.GetByIdAsync(trackId,restaurantId);

            if (track == null)
                return null;

            try
            {
                _uow.MusicTrack.Remove(track);
                await _uow.SaveChangesAsync();
                return track;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // rename music track title
        public async Task<bool> UpdateAsync(Guid trackId, int restaurantId, UpdateMusicTrackDto dto)
        {
            var track = await _uow.MusicTrack.GetByIdAsync(
                trackId,
                restaurantId);

            if (track == null)
                return false;

            track.Title = dto.Title;

            await _uow.MusicTrack.UpdateAsync(track);

            await _uow.SaveChangesAsync();

            return true;
        }
    }
}
