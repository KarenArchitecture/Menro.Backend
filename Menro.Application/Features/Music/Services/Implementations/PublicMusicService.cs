using Menro.Application.Common.Models;
using Menro.Application.Features.Music.DTOs.Public;
using Menro.Application.Features.Music.Enums;
using Menro.Domain.Entities.Music.Enums;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Music.Services.Interfaces;
using Menro.Application.Common.Interfaces;

namespace Menro.Application.Features.Music.Services.Implementations
{
    public class PublicMusicService : IPublicMusicService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMusicPlayerService _musicPlayerService;
        private readonly IMusicNotificationService _notifier;
        public PublicMusicService(
            IUnitOfWork uow, IMusicPlayerService musicPlayerService, IMusicNotificationService notifier)
        {
            _uow = uow;
            _musicPlayerService = musicPlayerService;
            _notifier = notifier;
        }

        public async Task<PublicMusicPageDto?> GetPageAsync(int restaurantId, string userId)
        {
            var playlist = await _uow.Playlist.GetActivePlaylistAsync(restaurantId);

            if (playlist is null)
                return null;

            var player = await _musicPlayerService.GetPlayerAsync(restaurantId);

            var todayRequests = await _uow.TrackRequest.GetTodayByRestaurantAsync(restaurantId);

            var hasRequestedToday = todayRequests.Any(x => x.UserId == userId);

            var requestLookup = todayRequests.GroupBy(x => x.MusicTrackId).ToDictionary(x => x.Key, x => x.First());

            var tracks = playlist.Tracks.OrderBy(x => x.SortOrder).Select(track =>
            {
                requestLookup.TryGetValue(track.MusicTrackId, out var request);

                var status = PublicTrackStatus.None;

                if (request is not null)
                {
                    status = request.UserId == userId ? PublicTrackStatus.MineRequested : PublicTrackStatus.Requested;
                }

                return new PublicTrackDto
                {
                    Id = track.Id,

                    Title = track.MusicTrack.Title,

                    Subtitle = track.MusicTrack.Artist,

                    ImageUrl /*(cover file name for just now)*/ = track.MusicTrack.CoverFileName,

                    IsCurrentTrack = player?.CurrentPlaylistTrackId == track.Id,

                    Status = status
                };
            })
            .ToList();

            return new PublicMusicPageDto
            {
                RemainingRequests = hasRequestedToday ? 0 : 1,

                CurrentTrackId = player?.CurrentPlaylistTrackId,

                Tracks = tracks
            };
        }

        public async Task<Result> RequestTrackAsync(int restaurantId, string userId, Guid playlistTrackId)
        {
            var playlist = await _uow.Playlist.GetActivePlaylistAsync(restaurantId);

            if (playlist is null)
                return Result.Failure("پلی‌لیست فعالی یافت نشد");

            var existsInPlaylist = playlist.Tracks.Any(x => x.Id == playlistTrackId);

            if (!existsInPlaylist)
                return Result.Failure("مشکلی رخ داده");

            var musicTrackId = await _uow.PlaylistTrack.GetMusicTrackIdAsync(playlistTrackId);

            //var hasRequestedToday = await _uow.TrackRequest.HasRequestedTodayAsync(restaurantId, userId);

            //if (hasRequestedToday)
            //    return Result.Failure(
            //        "شما امروز قبلاً درخواست ثبت کرده‌اید.");

            var request = new TrackRequest
            {
                Id = Guid.NewGuid(),

                RestaurantId = restaurantId,

                MusicTrackId = musicTrackId,

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
                    musicTrackId,
                    userId,
                    status = "Pending"
                });

                    return Result.Success();
            }
    }
}
