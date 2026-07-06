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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}