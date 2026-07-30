// Application/Features/Comments/DTOs/CommentDto.cs
namespace Menro.Application.Features.Comments.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Likes { get; set; }
        public bool Liked { get; set; }
        public CommentReplyDto? Reply { get; set; }
    }
}