using Menro.Domain.Entities;
using Menro.Domain.Enums;

namespace Menro.Domain.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<List<Comment>> GetApprovedCommentsByFoodIdAsync(int foodId);
        Task<Comment?> GetByIdAsync(int id);
        Task<List<Comment>> GetForAdminByStatusAsync(CommentStatus status);
        Task<Comment> AddCommentAsync(Comment comment);
        Task<CommentLike?> GetLikeAsync(int commentId, string userId, CommentLikeTarget target);
        Task AddLikeAsync(CommentLike like);
        Task RemoveLikeAsync(CommentLike like);
        Task<bool> UserAlreadyCommentedAsync(int foodId, string userId);
        Task SaveChangesAsync();
    }
}