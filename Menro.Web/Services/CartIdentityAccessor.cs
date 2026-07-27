using Menro.Application.Common.Interfaces;
using System.Security.Claims;

namespace Menro.Web.Services
{
    public class CartIdentityAccessor : ICartIdentityAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartIdentityAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true
                ? _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

        public string? GuestToken =>
            _httpContextAccessor.HttpContext?.Request.Headers["X-Guest-Cart-Id"].FirstOrDefault();
    }
}