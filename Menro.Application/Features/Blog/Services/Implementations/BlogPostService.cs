using Menro.Application.Common;
using Menro.Application.DTOs.Blog;
using Menro.Domain.Entities.Blog;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces.Blog;

namespace Menro.Application.Features.Blog.Services.Implementations
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IBlogPostRepository _repository;

        public BlogPostService(IBlogPostRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<BlogPostResponse>> GetAllAsync(
            string? search, BlogFeedCategory? category, CancellationToken ct = default)
        {
            var posts = await _repository.GetAllAsync(search, category, ct);
            return posts.Select(ToResponse).ToList();
        }

        public async Task<BlogPostResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(id, ct);
            return post is null ? null : ToResponse(post);
        }

        public async Task<BlogPostResponse> CreateAsync(CreateBlogPostRequest request, CancellationToken ct = default)
        {
            var post = new BlogPost
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                CoverImageUrl = request.CoverImageUrl,
                ReadingMinutes = request.ReadingMinutes,
                Category = request.Category,
                IsPublished = request.IsPublished,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _repository.AddAsync(post, ct);
            return ToResponse(post);
        }

        /// <returns>null if no post with that id exists.</returns>
        public async Task<BlogPostResponse?> UpdateAsync(
            Guid id, UpdateBlogPostRequest request, CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(id, ct);
            if (post is null) return null;

            post.Title = request.Title.Trim();
            post.CoverImageUrl = request.CoverImageUrl;
            post.ReadingMinutes = request.ReadingMinutes;
            post.Category = request.Category;
            post.IsPublished = request.IsPublished;

            await _repository.UpdateAsync(post, ct);
            return ToResponse(post);
        }

        public async Task<BlogPostResponse?> TogglePublishAsync(Guid id, CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(id, ct);
            if (post is null) return null;

            post.IsPublished = !post.IsPublished;
            await _repository.UpdateAsync(post, ct);
            return ToResponse(post);
        }

        /// <returns>false if no post with that id exists.</returns>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(id, ct);
            if (post is null) return false;

            await _repository.DeleteAsync(post, ct);
            return true;
        }

        private static BlogPostResponse ToResponse(BlogPost post) => new(
            post.Id,
            post.Title,
            post.CoverImageUrl,
            post.ReadingMinutes,
            post.Category,
            post.Category.ToLabel(),
            post.IsPublished,
            post.CreatedAtUtc,
            post.UpdatedAtUtc);
    }
}
