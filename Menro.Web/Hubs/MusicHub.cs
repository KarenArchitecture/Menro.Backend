using Microsoft.AspNetCore.SignalR;

namespace Menro.Web.Hubs;

public class MusicHub : Hub
{
    public async Task JoinRestaurant(int restaurantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(restaurantId)
        );
    }

    public static string GetGroupName(int restaurantId) => $"restaurant-{restaurantId}";
}