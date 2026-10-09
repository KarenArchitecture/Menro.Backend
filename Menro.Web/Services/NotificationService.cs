using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Music.DTOs.Notifications;
using Menro.Application.Features.Music.DTOs.Player;
using Menro.Application.Features.Orders.DTOs;
using Menro.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Menro.Web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<NotificationHub> hub,
            ILogger<NotificationService> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        /* MUSIC */

        public Task NotifyTrackRequested(int restaurantId, TrackRequestedNotification payload)
            => SafeSendAsync(
                () => _hub.Clients
                    .Group(NotificationHub.GetAdminGroupName(restaurantId))
                    .SendAsync(HubEvents.TrackRequested, payload),
                nameof(NotifyTrackRequested));

        public Task NotifyTrackApproved(string userId, TrackApprovedNotification payload)
            => SafeSendAsync(
                () => _hub.Clients
                    .User(userId)
                    .SendAsync(HubEvents.TrackApproved, payload),
                nameof(NotifyTrackApproved));

        public Task NotifyTrackRejected(string userId, TrackRejectedNotification payload)
            => SafeSendAsync(
                () => _hub.Clients
                    .User(userId)
                    .SendAsync(HubEvents.TrackRejected, payload),
                nameof(NotifyTrackRejected));

        public Task NotifyPlaylistChanged(int restaurantId)
            => SafeSendAsync(
                () => _hub.Clients
                    .Group(NotificationHub.GetGeneralGroupName(restaurantId))
                    .SendAsync(HubEvents.PlaylistChanged),
                nameof(NotifyPlaylistChanged));

        public Task NotifyPlaybackChanged(int restaurantId, MusicPlayerDto payload)
            => SafeSendAsync(
                () => _hub.Clients
                    .Group(NotificationHub.GetGeneralGroupName(restaurantId))
                    .SendAsync(HubEvents.PlaybackChanged, payload),
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

        /* ORDER */
        public Task NotifyOrderCreated(int restaurantId, OrderCreatedNotification payload)
            => SafeSendAsync(
                () => _hub.Clients
                    .Group(NotificationHub.GetAdminGroupName(restaurantId))
                    .SendAsync(HubEvents.OrderCreated, payload),
                nameof(NotifyOrderCreated));

    }
}