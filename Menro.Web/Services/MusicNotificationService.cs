using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Music.DTOs.Notifications;
using Menro.Application.Features.Music.DTOs.Player;
using Menro.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Menro.Web.Services
{
    public class MusicNotificationService : IMusicNotificationService
    {
        private readonly IHubContext<MusicHub> _hub;
        private readonly ILogger<MusicNotificationService> _logger;

        public MusicNotificationService(
            IHubContext<MusicHub> hub,
            ILogger<MusicNotificationService> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public Task NotifyTrackRequested(int restaurantId, TrackRequestedNotification payload)
            => SafeSendAsync(
                () => _hub.Clients
                    .Group(MusicHub.GetAdminGroupName(restaurantId))
                    .SendAsync(MusicHubEvents.TrackRequested, payload),
                nameof(NotifyTrackRequested));

        public Task NotifyTrackApproved(string userId, TrackApprovedNotification payload)
            => SafeSendAsync(
                () => _hub.Clients
                    .User(userId)
                    .SendAsync(MusicHubEvents.TrackApproved, payload),
                nameof(NotifyTrackApproved));

        public Task NotifyTrackRejected(string userId, TrackRejectedNotification payload)
            => SafeSendAsync(
                () => _hub.Clients
                    .User(userId)
                    .SendAsync(MusicHubEvents.TrackRejected, payload),
                nameof(NotifyTrackRejected));

        public Task NotifyPlaylistChanged(int restaurantId)
            => SafeSendAsync(
                () => _hub.Clients
                    .Group(MusicHub.GetGeneralGroupName(restaurantId))
                    .SendAsync(MusicHubEvents.PlaylistChanged),
                nameof(NotifyPlaylistChanged));

        public Task NotifyPlaybackChanged(int restaurantId, MusicPlayerDto payload)
            => SafeSendAsync(
                () => _hub.Clients
                    .Group(MusicHub.GetGeneralGroupName(restaurantId))
                    .SendAsync(MusicHubEvents.PlaybackChanged, payload),
                nameof(NotifyPlaybackChanged));

        private async Task SafeSendAsync(Func<Task> send, string operationName)
        {
            try
            {
                await send();
            }
            catch (Exception ex)
            {
                // نوتیفیکیشن real-time یک لایه‌ی best-effort روی state واقعیه؛
                // شکست خوردنش هیچ‌وقت نباید روی business flow اثر بذاره.
                _logger.LogError(ex,
                    "SignalR notification failed. Operation={Operation}", operationName);
            }
        }
    }
}