using Menro.Application.Common;
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Helpers;
using Menro.Domain.Entities.Blog;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces.Blog;

namespace Menro.Application.Features.Blog.Services.Implementations
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IBlogPostRepository _repository;
        private readonly IFileUrlService _fileUrlService;

        public BlogPostService(IBlogPostRepository repository, IFileUrlService fileUrlService)
        {
            _repository = repository;
            _fileUrlService = fileUrlService;
        }

        public async Task<PagedResult<BlogPostResponse>> GetAllAsync(
            string? search,
            Guid? categoryId,
            BlogPostSortOrder sort = BlogPostSortOrder.Newest,
            bool publishedOnly = false,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            var posts = await _repository.GetAllAsync(search, categoryId, ct);

            IEnumerable<BlogPost> query = posts;

            if (publishedOnly)
                query = query.Where(p => p.IsPublished);

            query = sort switch
            {
                BlogPostSortOrder.MostPopular => query.OrderByDescending(p => p.LikeCount),
                BlogPostSortOrder.MostViewed => query.OrderByDescending(p => p.ViewCount),
                _ => query.OrderByDescending(p => p.CreatedAtUtc),
            };

            var materialized = query.ToList();
            var totalCount = materialized.Count;

            // Guard against bad/absent query params rather than trusting the caller.
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

            var pageItems = materialized
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToResponse)
                .ToList();

            return new PagedResult<BlogPostResponse>
            {
                Items = pageItems,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
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
                // request.CoverImageUrl is actually just the file name returned by
                // the /posts/cover-image upload endpoint - never a full URL. See ToResponse.
                CoverImageUrl = request.CoverImageUrl,
                ReadingMinutes = request.ReadingMinutes,
                CategoryId = request.CategoryId,
                IsPublished = request.IsPublished,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _repository.AddAsync(post, ct);

            var created = await _repository.GetByIdAsync(post.Id, ct) ?? post;
            return ToResponse(created);
        }

        public async Task<BlogPostResponse?> UpdateAsync(
            Guid id, UpdateBlogPostRequest request, CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(id, ct);
            if (post is null) return null;

            post.Title = request.Title.Trim();
            post.CoverImageUrl = request.CoverImageUrl;
            post.ReadingMinutes = request.ReadingMinutes;
            post.CategoryId = request.CategoryId;
            post.IsPublished = request.IsPublished;

            await _repository.UpdateAsync(post, ct);

            var updated = await _repository.GetByIdAsync(post.Id, ct) ?? post;
            return ToResponse(updated);
        }

        public async Task<BlogPostResponse?> TogglePublishAsync(Guid id, CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(id, ct);
            if (post is null) return null;

            post.IsPublished = !post.IsPublished;
            await _repository.UpdateAsync(post, ct);
            return ToResponse(post);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(id, ct);
            if (post is null) return false;

            await _repository.DeleteAsync(post, ct);
            return true;
        }

        private BlogPostResponse ToResponse(BlogPost post) => new(
            post.Id,
            post.Title,
            string.IsNullOrWhiteSpace(post.CoverImageUrl)
                ? null
                : _fileUrlService.BuildBlogPostImageUrl(post.CoverImageUrl),
            post.ReadingMinutes,
            post.CategoryId,
            post.Category?.Title ?? string.Empty,
            post.IsPublished,
            post.CreatedAtUtc,
            post.UpdatedAtUtc,
            post.ViewCount,
            post.LikeCount,
            PersianDateHelper.ToPersianDisplayDate(post.CreatedAtUtc));
    }
}