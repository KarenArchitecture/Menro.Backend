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

            // NameClaimType در JWT options روی ClaimTypes.NameIdentifier ست شده،
            // پس این باید همیشه اولین match باشه. بقیه فقط fallback احتیاطین.
            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? user.FindFirstValue("sub")
                   ?? user.FindFirstValue("userid");
        }
    }
}