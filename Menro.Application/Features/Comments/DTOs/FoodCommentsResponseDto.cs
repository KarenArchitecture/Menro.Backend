// Application/Features/Comments/DTOs/FoodCommentsResponseDto.cs
namespace Menro.Application.Features.Comments.DTOs
{
    public class FoodCommentsResponseDto
    {
        public int FoodId { get; set; }
        public string FoodTitle { get; set; } = string.Empty;
        public string? FoodImageUrl { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string RestaurantSlug { get; set; } = string.Empty;
        public int ApprovedCommentsCount { get; set; }
        public bool HasUserCommented { get; set; }
        public string? UserCommentStatus { get; set; } // "Pending" | "Approved" | "Rejected" | null
        public List<CommentDto> Comments { get; set; } = new();
    }
}