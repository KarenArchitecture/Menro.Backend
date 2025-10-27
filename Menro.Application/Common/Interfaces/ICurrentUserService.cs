namespace Menro.Application.Common.Interfaces
{
    // move to application.common.interfaces later on
    public interface ICurrentUserService
    {
        string? GetUserId();
        Task<int> GetRestaurantIdAsync();
    }


}
