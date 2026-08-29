// Application/Comments/Services/Interfaces/CommentActionResult.cs
namespace Menro.Application.Comments.Services.Interfaces
{
    public enum CommentActionResult
    {
        Success,
        NotFound,
        Forbidden // comment exists but belongs to a different restaurant
    }
}