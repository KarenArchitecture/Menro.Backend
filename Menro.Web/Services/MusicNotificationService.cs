using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Music.DTOs.Player;
using Menro.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Menro.Web.Services
{
    public class MusicNotificationService : IMusicNotificationService
    {
        private readonly IHubContext<MusicHub> _hub;

        public MusicNotificationService(IHubContext<MusicHub> hub)
        {
            _hub = hub;
        }

        public Task NotifyTrackRequested(int restaurantId, object payload)
        {
            return _hub.Clients
                .Group(MusicHub.GetGroupName(restaurantId))
                .SendAsync("RequestCreated", payload);
        }

        public Task NotifyTrackApproved(string userId, object payload)
        {
            return _hub.Clients
                .User(userId)
                .SendAsync("RequestApproved", payload);
        }

        public Task NotifyTrackRejected(string userId, object payload)
        {
            return _hub.Clients
                .User(userId)
                .SendAsync("RequestRejected", payload);
        }

        public async Task NotifyPlaylistChanged(int restaurantId)
        {
            await _hub.Clients.Group(MusicHub.GetGroupName(restaurantId)).SendAsync("PlaylistChanged");
        }

        public async Task NotifyPlaybackChanged(int restaurantId, MusicPlayerDto payload)
        {
            await _hub.Clients
                .Group(MusicHub.GetGroupName(restaurantId))
                .SendAsync("PlaybackChanged", payload);
        }
    }
}