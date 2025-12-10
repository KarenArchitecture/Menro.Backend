namespace Menro.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string? GetUserId();
        Task<int> GetRestaurantIdAsync();
    }


}
