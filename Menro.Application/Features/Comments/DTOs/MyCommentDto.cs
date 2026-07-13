// Application/Features/Comments/DTOs/MyCommentDto.cs
using Menro.Application.Features.Comments.DTOs;

public class MyCommentDto
{
    public int Id { get; set; }
    public int FoodId { get; set; }
    public string FoodTitle { get; set; } = string.Empty;
    public string? FoodImageUrl { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string RestaurantSlug { get; set; } = string.Empty;
    public int ApprovedCommentsCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int Likes { get; set; }
    public bool Liked { get; set; }
    public CommentReplyDto? Reply { get; set; }
}