namespace Menro.Application.Common.Interfaces
{
    public interface ICartIdentityAccessor
    {
        string? UserId { get; }
        string? GuestToken { get; }
    }
}