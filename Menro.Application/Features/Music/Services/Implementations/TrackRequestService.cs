using Menro.Application.Common.Interfaces;
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

            request.Status = TrackRequestStatus.Rejected;

            await _uow.TrackRequest.UpdateAsync(request);
            await _uow.SaveChangesAsync();

            await _notifier.NotifyTrackRejected(
                restaurantId,
                new
                {
                    id = request.Id,
                    musicTrackId = request.MusicTrackId,
                    status = "Rejected"
                });

            return true;
        }


        public async Task<bool> ApproveAsync(Guid requestId, int restaurantId)
        {
            // 1. Request
            var request = await _uow.TrackRequest.GetByIdAsync(requestId);

            if (request == null || request.RestaurantId != restaurantId)
                return false;

            if (request.Status != TrackRequestStatus.Pending)
                return false;

            // 2. Player
            var player = await _uow.MusicPlayer.GetByRestaurantIdAsync(restaurantId);

            if (player == null)
                return false;

            // 3. Active playlist (source of truth)
            var playlist = await _uow.Playlist.GetActiveByRestaurantIdAsync(restaurantId);

            if (playlist == null)
                throw new Exception("No active playlist found");

            var playlistId = playlist.Id;

            // 4. Current track
            var current = player.CurrentPlaylistTrackId.HasValue
                ? await _uow.PlaylistTrack.GetByIdAsync(player.CurrentPlaylistTrackId.Value)
                : null;

            var currentSortOrder = current?.SortOrder ?? 0;

            // 5. Find last requested track AFTER current
            var requestedTracks = await _uow.PlaylistTrack.GetRequestedTracksAfterCurrentAsync(playlistId, currentSortOrder);

            var lastRequested = requestedTracks.LastOrDefault();

            // 6. Insert position
            var insertAfterSortOrder = lastRequested?.SortOrder ?? currentSortOrder;

            // 7. Shift tracks in ONE batch (important fix)
            var tracksToShift = await _uow.PlaylistTrack.GetAfterSortOrderAsync(playlistId, insertAfterSortOrder);

            foreach (var track in tracksToShift)
            {
                track.SortOrder += 1;
            }

            // 8. Create new requested track
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

            // 9. Update request
            request.Status = TrackRequestStatus.Approved;
            await _uow.TrackRequest.UpdateAsync(request);

            // 10. Single save
            await _uow.SaveChangesAsync();

            await _notifier.NotifyTrackApproved(
                restaurantId,
                new
                {
                    id = request.Id,
                    musicTrackId = request.MusicTrackId,
                    status = "Approved"
                });

            return true;
        }
    }
}
