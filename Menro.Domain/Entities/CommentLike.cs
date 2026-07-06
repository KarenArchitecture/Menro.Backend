using Menro.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities
{
    public class CommentLike
    {
        [Key]
        public int Id { get; set; }

        public int CommentId { get; set; }
        public Comment Comment { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        public CommentLikeTarget Target { get; set; } = CommentLikeTarget.Comment;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}