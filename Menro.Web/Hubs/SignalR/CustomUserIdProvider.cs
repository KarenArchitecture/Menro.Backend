using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Menro.Web.Hubs.SignalR
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var user = connection.User;

            if (user == null)
                return null;

            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? user.FindFirstValue("sub")
                   ?? user.FindFirstValue("userid");
        }
    }
}