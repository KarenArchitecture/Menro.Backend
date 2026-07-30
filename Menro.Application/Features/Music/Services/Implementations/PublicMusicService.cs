using Menro.Application.Common.Models;
using Menro.Application.Features.Music.DTOs.Public;
using Menro.Application.Features.Music.Enums;
using Menro.Domain.Entities.Music.Enums;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Music.Services.Interfaces;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;

namespace Menro.Application.Features.Music.Services.Implementations
{
    public class PublicMusicService : IPublicMusicService
    {
        #region DI
        private readonly IUnitOfWork _uow;
        private readonly IMusicPlayerService _musicPlayerService;
        private readonly IMusicNotificationService _notifier;
        private readonly IMediaStorageProvider _mediaStorage;
        public PublicMusicService(
            IUnitOfWork uow,
            IMusicPlayerService musicPlayerService,
            IMusicNotificationService notifier,
            IMediaStorageProvider mediaStorage)
        {
            _uow = uow;
            _musicPlayerService = musicPlayerService;
            _notifier = notifier;
            _mediaStorage = mediaStorage;
        }
        #endregion

        public async Task<PublicMusicPageDto?> GetPageAsync(int restaurantId, string userId)
        {
            var playlist =await _uow.Playlist.GetActivePlaylistAsync(restaurantId);
            var player = await _musicPlayerService.GetPlayerAsync(restaurantId);

            if (playlist is null || player is null)
                return null;

            // Daily limit
            var today = DateTime.UtcNow.Date;

            var todayRequests = await _uow.TrackRequest.GetTodayByRestaurantAsync(restaurantId);

            var myTodayRequests = todayRequests.Count(x => x.UserId == userId);

            var remainingRequests = Math.Max(0, 50 - myTodayRequests);

            // فقط درخواست‌هایی که به PlaylistTrack وصل شده‌اند
            var requestIds = playlist.Tracks
                .Where(x =>
                    x.IsRequestedTrack &&
                    x.TrackRequestId.HasValue)
                .Select(x => x.TrackRequestId!.Value)
                .ToList();

            var requests = await _uow.TrackRequest.GetByIdsAsync(requestIds);

            var requestLookup = requests.ToDictionary(x => x.Id);

            var tracks = playlist.Tracks
                .OrderBy(x => x.SortOrder)
                .Select(track =>
                {
                    var status = PublicTrackStatus.None;

                    if (track.IsRequestedTrack &&
                        track.TrackRequestId.HasValue &&
                        requestLookup.TryGetValue(
                            track.TrackRequestId.Value,
                            out var request))
                    {
                        status =
                            request.UserId == userId
                            ? PublicTrackStatus.MineRequested
                            : PublicTrackStatus.Requested;
                    }

                    return new PublicTrackDto
                    {
                        Id = track.Id,

                        Title = track.MusicTrack.Title,

                        Subtitle = track.MusicTrack.Artist,

                        ImageUrl = string.IsNullOrWhiteSpace(track.MusicTrack.CoverFileName)
                            ? null
                            : _mediaStorage.GetUrl(MediaCategory.RestaurantMusicCover, track.MusicTrack.CoverFileName, restaurantId.ToString(), MediaVariant.Thumbnail),
                        IsCurrentTrack =
                            player?.CurrentTrackId == track.Id,

                        Status = status
                    };
                })
                .ToList();

            return new PublicMusicPageDto
            {
                RemainingRequests = remainingRequests,

                CurrentTrackId = player?.CurrentTrackId,

                Tracks = tracks
            };
        }

        public async Task<Result> RequestTrackAsync(int restaurantId, string userId, Guid playlistTrackId)
        {
            if (string.IsNullOrEmpty(userId))
                return Result.Failure("کاربر معتبر نیست");

            var playlist = await _uow.Playlist.GetActivePlaylistAsync(restaurantId);

            if (playlist is null)
                return Result.Failure("پلی‌لیست فعالی یافت نشد");

            var playlistTrack = playlist.Tracks.FirstOrDefault(x => x.Id == playlistTrackId);

            if (playlistTrack is null)
                return Result.Failure("مشکلی رخ داده");

            var request = new TrackRequest
            {
                Id = Guid.NewGuid(),

                RestaurantId = restaurantId,

                MusicTrackId = playlistTrack.MusicTrackId,

                PlaylistTrackId = playlistTrackId,

                UserId = userId,

                RequestedAt = DateTime.UtcNow,

                Status = TrackRequestStatus.Pending
            };

            await _uow.TrackRequest.AddAsync(request);

            await _uow.SaveChangesAsync();

            await _notifier.NotifyTrackRequested(
                restaurantId,
                new
                {
                    id = request.Id,
                    playlistTrackId,
                    userId,
                    status = "Pending"
                });

            return Result.Success();
        }
    }
}
