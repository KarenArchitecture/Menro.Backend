using Menro.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Menro.Web.Hubs
{
    [Authorize]
    public class MusicHub : Hub
    {
        private readonly IRestaurantAdminAccessService _adminAccess;
        private readonly ILogger<MusicHub> _logger;

        public MusicHub(
            IRestaurantAdminAccessService adminAccess,
            ILogger<MusicHub> logger)
        {
            _adminAccess = adminAccess;
            _logger = logger;
        }

        // ---- Customer side ----
        public async Task JoinAsCustomer(int restaurantId)
        {
            if (restaurantId <= 0)
                throw new HubException("شناسه رستوران نامعتبر است");

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetGeneralGroupName(restaurantId));
        }

        public async Task LeaveAsCustomer(int restaurantId)
        {
            if (restaurantId <= 0) return;

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                GetGeneralGroupName(restaurantId));
        }

        // ---- Admin side ----
        [Authorize(Roles = "Admin")]
        public async Task JoinAsAdmin(int restaurantId)
        {
            if (restaurantId <= 0)
                throw new HubException("شناسه رستوران نامعتبر است");

            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                throw new HubException("کاربر شناسایی نشد");

            var allowed = await _adminAccess.IsAdminOfRestaurantAsync(userId, restaurantId);
            if (!allowed)
            {
                _logger.LogWarning(
                    "Admin {UserId} tried to join restaurant {RestaurantId} without access",
                    userId, restaurantId);
                throw new HubException("دسترسی به این رستوران ندارید");
            }

                 // ادمین هم باید عضو گروه عمومی باشه تا PlaybackChanged/PlaylistChanged رو دریافت کنه
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetGeneralGroupName(restaurantId));

            
            // و باید عضو گروه اختصاصی ادمین هم باشه تا RequestCreated رو دریافت کنه
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetAdminGroupName(restaurantId));
        }

        [Authorize(Roles = "Admin")]
        public async Task LeaveAsAdmin(int restaurantId)
        {
            if (restaurantId <= 0) return;

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                GetGeneralGroupName(restaurantId));

           
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                GetAdminGroupName(restaurantId));
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogDebug(
                "SignalR connected. ConnectionId={ConnectionId} UserId={UserId}",
                Context.ConnectionId, Context.UserIdentifier);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception is null)
            {
                _logger.LogDebug(
                    "SignalR disconnected. ConnectionId={ConnectionId} UserId={UserId}",
                    Context.ConnectionId, Context.UserIdentifier);
            }
            else
            {
                _logger.LogWarning(exception,
                    "SignalR disconnected with error. ConnectionId={ConnectionId} UserId={UserId}",
                    Context.ConnectionId, Context.UserIdentifier);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public static string GetGeneralGroupName(int restaurantId)
            => $"restaurant-{restaurantId}";

        public static string GetAdminGroupName(int restaurantId)
            => $"restaurant-{restaurantId}-admins";
    }
}