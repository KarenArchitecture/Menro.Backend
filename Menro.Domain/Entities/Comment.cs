using Menro.Domain.Enums;
using Menro.Domain.Interfaces.Persistence;
using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities
{
    public class Comment : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        public int FoodId { get; set; }
        public Food Food { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required(ErrorMessage = "متن نظر الزامی است")]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        public CommentStatus Status { get; set; } = CommentStatus.Pending;

        [MaxLength(500)]
        public string? RejectReason { get; set; }

        [MaxLength(1000)]
        public string? ReplyText { get; set; }
        public DateTime? ReplyDate { get; set; }

        public int LikesCount { get; set; } = 0;
        public int ReplyLikesCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    }
}