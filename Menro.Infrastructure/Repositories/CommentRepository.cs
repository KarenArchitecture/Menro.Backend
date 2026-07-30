using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Repositories
{
    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        private readonly MenroDbContext _context;

        public CommentRepository(MenroDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Comment>> GetApprovedCommentsByFoodIdAsync(int foodId)
        {
            return await _context.Comments
                .Where(c => c.FoodId == foodId && c.Status == CommentStatus.Approved)
                .Include(c => c.Food)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Comment>> GetForAdminByStatusAsync(CommentStatus status)
        {
            return await _context.Comments
                .Where(c => c.Status == status)
                .Include(c => c.Food)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            return comment;
        }

        public async Task<CommentLike?> GetLikeAsync(int commentId, string userId, CommentLikeTarget target)
        {
            return await _context.CommentLikes.FirstOrDefaultAsync(l =>
                l.CommentId == commentId && l.UserId == userId && l.Target == target);
        }

        public async Task AddLikeAsync(CommentLike like)
        {
            await _context.CommentLikes.AddAsync(like);
        }

        public async Task RemoveLikeAsync(CommentLike like)
        {
            _context.CommentLikes.Remove(like);
            await Task.CompletedTask;
        }

        public async Task<bool> UserAlreadyCommentedAsync(int foodId, string userId)
        {
            return await _context.Comments.AnyAsync(c => c.FoodId == foodId && c.UserId == userId);
        }

        public async Task<int> GetApprovedCountByFoodIdAsync(int foodId)
        {
            return await _context.Comments
                .CountAsync(c => c.FoodId == foodId && c.Status == CommentStatus.Approved);
        }

        public async Task<FoodSummaryResult?> GetFoodSummaryAsync(int foodId)
        {
            var food = await _context.Foods
                .AsNoTracking()
                .Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f => f.Id == foodId);

            if (food == null) return null;

            return new FoodSummaryResult
            {
                Title = food.Name,
                ImageUrl = food.ImageUrl,
                RestaurantName = food.Restaurant?.Name ?? string.Empty,
                RestaurantSlug = food.Restaurant?.Slug ?? string.Empty
            };
        }

        public async Task<List<Comment>> GetByUserIdAsync(string userId)
        {
            return await _context.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.Food)
                    .ThenInclude(f => f.Restaurant)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetApprovedCountsByFoodIdsAsync(IEnumerable<int> foodIds)
        {
            return await _context.Comments
                .Where(c => foodIds.Contains(c.FoodId) && c.Status == CommentStatus.Approved)
                .GroupBy(c => c.FoodId)
                .Select(g => new { FoodId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.FoodId, x => x.Count);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}