using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Music.DTOs.Notifications;
using Menro.Application.Features.Music.DTOs.Requests;
using Menro.Application.Features.Music.Services.Interfaces;
using Menro.Domain.Entities.Music;
using Menro.Domain.Entities.Music.Enums;
using Menro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Menro.Application.Features.Music.Services.Implementations
{
    public class TrackRequestService : ITrackRequestService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMusicNotificationService _notifier;

        public TrackRequestService(IUnitOfWork uow, IMusicNotificationService notifier)
        {
            _uow = uow;
            _notifier = notifier;
        }

        public async Task<List<RequestedTrackDto>> GetPendingAsync(int restaurantId)
        {
            var requests = await _uow.TrackRequest.GetPendingByRestaurantIdAsync(restaurantId);

            return requests.Select(x => new RequestedTrackDto
            {
                Id = x.Id,
                MusicTrackId = x.MusicTrackId,
                Title = x.MusicTrack.Title,
                Artist = x.MusicTrack.Artist
            }).ToList();
        }


        public async Task<bool> RejectAsync(Guid requestId, int restaurantId)
        {
            var request = await _uow.TrackRequest.GetByIdAsync(requestId);

            if (request == null)
                return false;

            if (request.RestaurantId != restaurantId)
                return false;

            if (request.Status != TrackRequestStatus.Pending)
                return false;

            request.Status = TrackRequestStatus.Rejected;

            await _uow.TrackRequest.UpdateAsync(request);
            await _uow.SaveChangesAsync();

            await _notifier.NotifyTrackRejected(
                request.UserId,
                new TrackRejectedNotification
                {
                    RequestId = request.Id,
                    Reason = null, // اگه بعداً فیلد دلیل رد کردن اضافه کردی، اینجا پاسش بده
                    RejectedAt = DateTime.UtcNow
                });

            return true;
        }

        public async Task<bool> ApproveAsync(Guid requestId, int restaurantId)
        {
            var request = await _uow.TrackRequest.GetByIdAsync(requestId);

            if (request == null || request.RestaurantId != restaurantId)
                return false;

            if (request.Status != TrackRequestStatus.Pending)
                return false;

            var player = await _uow.MusicPlayer.GetByRestaurantIdAsync(restaurantId);

            if (player == null)
                return false;

            var playlist = await _uow.Playlist.GetActiveByRestaurantIdAsync(restaurantId);

            if (playlist == null)
                throw new Exception("No active playlist found");

            var playlistId = playlist.Id;

            var current = player.CurrentPlaylistTrackId.HasValue
                ? await _uow.PlaylistTrack.GetByIdAsync(player.CurrentPlaylistTrackId.Value)
                : null;

            var currentSortOrder = current?.SortOrder ?? 0;

            var requestedTracks =
                await _uow.PlaylistTrack.GetRequestedTracksAfterCurrentAsync(
                    playlistId,
                    currentSortOrder);

            var lastRequested = requestedTracks.LastOrDefault();

            var insertAfterSortOrder = lastRequested?.SortOrder ?? currentSortOrder;

            var tracksToShift =
                await _uow.PlaylistTrack.GetAfterSortOrderAsync(
                    playlistId,
                    insertAfterSortOrder);

            foreach (var track in tracksToShift)
            {
                track.SortOrder += 1;
            }

            var newTrack = new PlaylistTrack
            {
                Id = Guid.NewGuid(),
                PlaylistId = playlistId,
                MusicTrackId = request.MusicTrackId,
                SortOrder = insertAfterSortOrder + 1,
                IsRequestedTrack = true,
                TrackRequestId = request.Id
            };

            await _uow.PlaylistTrack.AddAsync(newTrack);

            request.Status = TrackRequestStatus.Approved;
            request.PlaylistTrackId = newTrack.Id;

            await _uow.TrackRequest.UpdateAsync(request);
            await _uow.SaveChangesAsync();

            await _notifier.NotifyTrackApproved(
                request.UserId,
                new TrackApprovedNotification
                {
                    RequestId = request.Id,
                    PlaylistTrackId = newTrack.Id,
                    ApprovedAt = DateTime.UtcNow
                });

            // وقتی ترک approve میشه یک PlaylistTrack جدید هم اضافه شده،
            // یعنی پلی‌لیست عملاً برای مشتری‌ها هم عوض شده — این باید هم اطلاع داده بشه
            await _notifier.NotifyPlaylistChanged(restaurantId);

            return true;
        }
    }
}
