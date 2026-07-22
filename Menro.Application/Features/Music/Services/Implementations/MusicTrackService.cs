using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Music.DTOs.Archive;
using Menro.Application.Features.Music.DTOs.Player;
using Menro.Application.Features.Music.Services.Interfaces;
using Menro.Application.Helpers;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Music.Services.Implementations
{
    internal class MusicTrackService : IMusicTrackService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMusicNotificationService _musicNotificationService;
        private readonly IMediaStorageProvider _mediaStorage;

        public MusicTrackService(
            IUnitOfWork uow,
            IMusicNotificationService musicNotificationService,
            IMediaStorageProvider mediaStorage)
        {
            _uow = uow;
            _musicNotificationService = musicNotificationService;
            _mediaStorage = mediaStorage;
        }

        // add music (upload + metadata extraction + persist, all in one place)
        public async Task<MusicTrackListItemDto> CreateAsync(int restaurantId, IFormFile audioFile, IFormFile? coverFile)
        {
            if (audioFile == null || audioFile.Length == 0)
                throw new ArgumentException("فایل موسیقی ارسال نشده است.");

            string? audioFileName = null;
            string? coverFileName = null;

            try
            {
                // اعتبارسنجی فرمت/سایز اینجا انجام نمیشه؛ SaveAsync طبق MediaCategoryRegistry
                // خودش InvalidOperationException با پیام مناسب می‌اندازه اگه فایل نامعتبر باشه.
                var audioResult = await _mediaStorage.SaveAsync(MediaCategory.RestaurantMusicFile, audioFile);
                audioFileName = audioResult.FileName;

                var audioPath = _mediaStorage.GetPhysicalPath(MediaCategory.RestaurantMusicFile, audioFileName);
                var metadata = AudioMetadataExtractor.Extract(audioPath);

                if (coverFile != null)
                {
                    var coverResult = await _mediaStorage.SaveAsync(MediaCategory.RestaurantMusicCover, coverFile);
                    coverFileName = coverResult.FileName;
                }
                else
                {
                    var coverBytes = AudioMetadataExtractor.ExtractCover(audioPath);
                    if (coverBytes != null)
                    {
                        var coverResult = await _mediaStorage.SaveBytesAsync(
                            MediaCategory.RestaurantMusicCover, coverBytes, ".jpg");
                        coverFileName = coverResult.FileName;
                    }
                }

                var track = new MusicTrack
                {
                    Id = Guid.NewGuid(),
                    RestaurantId = restaurantId,
                    Title = metadata.Title,
                    Artist = metadata.Artist,
                    Duration = metadata.Duration,
                    AudioFileName = audioFileName,
                    CoverFileName = coverFileName
                };

                await _uow.MusicTrack.AddAsync(track);
                await _uow.SaveChangesAsync();

                return new MusicTrackListItemDto
                {
                    Id = track.Id,
                    Title = track.Title,
                    Artist = track.Artist,
                    Duration = track.Duration,
                    CoverFileName = coverFileName == null
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantMusicCover, coverFileName)
                };
            }
            catch
            {
                // rollback: هر خطایی (اعتبارسنجی، استخراج متادیتا، شکست DB) → پاک‌سازی فایل‌های آپلودشده
                if (!string.IsNullOrWhiteSpace(audioFileName))
                    _mediaStorage.Delete(MediaCategory.RestaurantMusicFile, audioFileName);

                if (!string.IsNullOrWhiteSpace(coverFileName))
                    _mediaStorage.Delete(MediaCategory.RestaurantMusicCover, coverFileName);

                throw;
            }
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
                CoverFileName = string.IsNullOrWhiteSpace(t.CoverFileName)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantMusicCover, t.CoverFileName)
            }).ToList();
        }

        // get music metadata (NOT the audio bytes - AudioUrl points to the protected /stream endpoint)
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

                // فایل صوتی public نیست؛ آدرس endpoint محافظت‌شده داده میشه (مطلق، نه نسبی)
                AudioUrl = $"{_mediaStorage.GetBaseUrl()}/api/admin/music/archive/{track.Id}/stream",

                CoverUrl = string.IsNullOrWhiteSpace(track.CoverFileName)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantMusicCover, track.CoverFileName)
            };
        }

        // physical path used ONLY by the controller's stream action (PhysicalFile needs a path, not a URL)
        public async Task<string?> GetAudioPhysicalPathAsync(Guid trackId, int restaurantId)
        {
            var track = await _uow.MusicTrack.GetByIdAsync(trackId, restaurantId);

            if (track == null || string.IsNullOrWhiteSpace(track.AudioFileName))
                return null;

            return _mediaStorage.GetPhysicalPath(MediaCategory.RestaurantMusicFile, track.AudioFileName);
        }

        // remove music
        public async Task<MusicTrack?> RemoveAsync(Guid trackId, int restaurantId)
        {
            var track = await _uow.MusicTrack.GetByIdAsync(trackId, restaurantId);

            if (track == null)
                return null;

            try
            {
                bool playbackChanged = false;

                var player = await _uow.MusicPlayer.GetByRestaurantIdAsync(restaurantId);
                var playlistTracks = await _uow.PlaylistTrack.GetAllByMusicTrackId(trackId);
                var trackRequests = await _uow.TrackRequest.GetAllByMusicTrackId(trackId);

                if (player?.CurrentPlaylistTrackId != null)
                {
                    var currentPlaylistTrack = playlistTracks.FirstOrDefault(
                        x => x.Id == player.CurrentPlaylistTrackId);

                    if (currentPlaylistTrack != null)
                    {
                        var nextTrack = await _uow.PlaylistTrack.GetNextTrackAsync(
                            currentPlaylistTrack.PlaylistId,
                            currentPlaylistTrack.SortOrder);

                        if (nextTrack != null)
                        {
                            player.CurrentPlaylistTrackId = nextTrack.Id;
                            player.PlaylistId = nextTrack.PlaylistId;
                        }
                        else
                        {
                            player.CurrentPlaylistTrackId = null;
                            player.PlaylistId = null;
                        }

                        player.LastUpdatedAt = DateTime.UtcNow;
                        await _uow.MusicPlayer.UpdateAsync(player);
                        playbackChanged = true;
                    }
                }

                await _uow.PlaylistTrack.RemoveRange(playlistTracks);
                await _uow.TrackRequest.RemoveRange(trackRequests);

                _uow.MusicTrack.Remove(track);
                await _uow.SaveChangesAsync();

                // فایل‌های فیزیکی را همین‌جا (بعد از موفقیت حذف رکورد) پاک می‌کنیم
                if (!string.IsNullOrWhiteSpace(track.AudioFileName))
                    _mediaStorage.Delete(MediaCategory.RestaurantMusicFile, track.AudioFileName);

                if (!string.IsNullOrWhiteSpace(track.CoverFileName))
                    _mediaStorage.Delete(MediaCategory.RestaurantMusicCover, track.CoverFileName);

                if (playlistTracks.Any())
                    await _musicNotificationService.NotifyPlaylistChanged(restaurantId);

                if (playbackChanged)
                {
                    await _musicNotificationService.NotifyPlaybackChanged(
                        restaurantId,
                        new MusicPlayerDto
                        {
                            PlaylistId = player?.PlaylistId,
                            CurrentTrackId = player?.CurrentPlaylistTrackId
                        });
                }

                return track;
            }
            catch
            {
                return null;
            }
        }

        // rename music track title
        public async Task<bool> UpdateAsync(Guid trackId, int restaurantId, UpdateMusicTrackDto dto)
        {
            var track = await _uow.MusicTrack.GetByIdAsync(trackId, restaurantId);

            if (track == null)
                return false;

            track.Title = dto.Title;

            await _uow.MusicTrack.UpdateAsync(track);
            await _uow.SaveChangesAsync();

            return true;
        }
    }
}