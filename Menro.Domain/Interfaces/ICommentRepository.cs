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
        Task<Comment?> GetUserCommentForFoodAsync(int foodId, string userId);
        Task<bool> UserAlreadyCommentedAsync(int foodId, string userId);
        Task SaveChangesAsync();

        Task<int> GetApprovedCountByFoodIdAsync(int foodId);
        Task<FoodSummaryResult?> GetFoodSummaryAsync(int foodId);
        Task<List<Comment>> GetByUserIdAsync(string userId);
        Task<Dictionary<int, int>> GetApprovedCountsByFoodIdsAsync(IEnumerable<int> foodIds);
    }

    public class FoodSummaryResult
    {
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string RestaurantSlug { get; set; } = string.Empty;
    }
}