using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories;

public class FavoriteFoodRepository : IFavoriteFoodRepository
{
    private readonly MenroDbContext _db;

    public FavoriteFoodRepository(MenroDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsAsync(string userId, int foodId)
    {
        return await _db.FavoriteFoods
            .AnyAsync(x => x.UserId == userId && x.FoodId == foodId);
    }

    public async Task AddAsync(FavoriteFood favoriteFood)
    {
        await _db.FavoriteFoods.AddAsync(favoriteFood);
    }

    public async Task RemoveAsync(FavoriteFood favoriteFood)
    {
        _db.FavoriteFoods.Remove(favoriteFood);
    }

    public async Task<FavoriteFood?> GetAsync(string userId, int foodId)
    {
        return await _db.FavoriteFoods
            .FirstOrDefaultAsync(x => x.UserId == userId && x.FoodId == foodId);
    }

    public async Task<List<int>> GetFavoriteFoodIdsByUserAsync(string userId)
    {
        return await _db.FavoriteFoods
            .Where(x => x.UserId == userId)
            .Select(x => x.FoodId)
            .ToListAsync();
    }
}