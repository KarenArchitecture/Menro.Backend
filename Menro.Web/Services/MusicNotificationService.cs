using Menro.Application.Common.Interfaces;
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
    }
}
