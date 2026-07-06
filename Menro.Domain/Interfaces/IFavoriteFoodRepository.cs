using Menro.Domain.Entities;

namespace Menro.Domain.Interfaces;

public interface IFavoriteFoodRepository
{
    Task<bool> ExistsAsync(
        string userId,
        int foodId);

    Task AddAsync(
        FavoriteFood favoriteFood);

    Task RemoveAsync(
        FavoriteFood favoriteFood);

    Task<FavoriteFood?> GetAsync(
        string userId,
        int foodId);

    Task<List<int>> GetFavoriteFoodIdsByUserAsync(string userId);
}