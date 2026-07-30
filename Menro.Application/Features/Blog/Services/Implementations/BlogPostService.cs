using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Helpers;
using Menro.Domain.Entities.Blog;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces.Blog;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Blog.Services.Implementations
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IBlogPostRepository _repository;
        private readonly IMediaStorageProvider _mediaStorage;

        public BlogPostService(IBlogPostRepository repository, IMediaStorageProvider mediaStorage)
        {
            _repository = repository;
            _mediaStorage = mediaStorage;
        }

        public async Task<PagedResult<BlogPostResponse>> GetAllAsync(
            string? search,
            Guid? categoryId,
            Guid? tagId = null,
            BlogPostSortOrder sort = BlogPostSortOrder.Newest,
            bool publishedOnly = false,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            var posts = await _repository.GetAllAsync(search, categoryId, tagId, ct);

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

        // add blog post - combined upload + create
        public async Task<BlogPostResponse> CreateAsync(CreateBlogPostRequest request, CancellationToken ct = default)
        {
            var id = Guid.NewGuid();
            var entityId = id.ToString();
            string? coverFileName = null;

            try
            {
                if (request.CoverImage is not null && request.CoverImage.Length > 0)
                {
                    var uploadResult = await _mediaStorage.SaveAsync(
                        MediaCategory.BlogPostImage, request.CoverImage, entityId, ct: ct);
                    coverFileName = uploadResult.FileName;
                }

                var post = new BlogPost
                {
                    Id = id,
                    Title = request.Title.Trim(),
                    CoverImageUrl = coverFileName,
                    ReadingMinutes = request.ReadingMinutes,
                    CategoryId = request.CategoryId,
                    IsPublished = request.IsPublished,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _repository.AddAsync(post, ct);

                var created = await _repository.GetByIdAsync(post.Id, ct) ?? post;
                return ToResponse(created);
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(coverFileName))
                    _mediaStorage.Delete(MediaCategory.BlogPostImage, coverFileName, entityId);
                throw;
            }
        }

        // update blog post - combined update + image replace/remove در یک فراخوانی
        public async Task<BlogPostResponse?> UpdateAsync(
            Guid id, UpdateBlogPostRequest request, CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(id, ct);
            if (post is null) return null;

            post.Title = request.Title.Trim();
            post.ReadingMinutes = request.ReadingMinutes;
            post.CategoryId = request.CategoryId;
            post.IsPublished = request.IsPublished;

            var entityId = id.ToString();

            if (request.RemoveImage)
            {
                if (!string.IsNullOrWhiteSpace(post.CoverImageUrl))
                    _mediaStorage.Delete(MediaCategory.BlogPostImage, post.CoverImageUrl, entityId);
                post.CoverImageUrl = null;
            }
            else if (request.CoverImage is not null && request.CoverImage.Length > 0)
            {
                // SaveAsync خودش نسخه‌ی قدیمی (همه‌ی وریانت‌هاش) رو قبل از نوشتن جدید پاک می‌کنه
                var uploadResult = await _mediaStorage.SaveAsync(
                    MediaCategory.BlogPostImage,
                    request.CoverImage,
                    entityId,
                    oldFileName: post.CoverImageUrl,
                    ct: ct);

                post.CoverImageUrl = uploadResult.FileName;
            }
            // else: نه CoverImage اومده نه RemoveImage - عکس فعلی دست‌نخورده می‌مونه

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

            if (!string.IsNullOrWhiteSpace(post.CoverImageUrl))
                _mediaStorage.Delete(MediaCategory.BlogPostImage, post.CoverImageUrl, id.ToString());

            return true;
        }

        private BlogPostResponse ToResponse(BlogPost post) => new(
            post.Id,
            post.Title,
            string.IsNullOrWhiteSpace(post.CoverImageUrl)
                ? null
                : _mediaStorage.GetUrl(MediaCategory.BlogPostImage, post.CoverImageUrl, post.Id.ToString(), MediaVariant.Thumbnail),
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