using Microsoft.AspNetCore.SignalR;

namespace Menro.Web.Hubs
{
    public class MusicHub : Hub
    {
        public async Task JoinRestaurant(int restaurantId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetGroupName(restaurantId)
            );
        }
        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var claims = Context.User?.Claims.Select(c => $"{c.Type}:{c.Value}");

            Console.WriteLine("USER ID:");
            Console.WriteLine(userId);

            Console.WriteLine("CLAIMS:");
            foreach (var c in claims ?? [])
                Console.WriteLine(c);

            return base.OnConnectedAsync();
        }

        public static string GetGroupName(int restaurantId)
            => $"restaurant-{restaurantId}";
    }
}

