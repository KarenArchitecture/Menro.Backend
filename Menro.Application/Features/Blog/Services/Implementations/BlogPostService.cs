using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Helpers;
using Menro.Domain.Entities.Blog;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Blog.Services.Implementations
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediaStorageProvider _mediaStorage;

        public BlogPostService(IUnitOfWork unitOfWork, IMediaStorageProvider mediaStorage)
        {
            _unitOfWork = unitOfWork;
            _mediaStorage = mediaStorage;
        }

        public async Task<PagedResult<BlogPostResponse>> GetAllAsync(
            string? search, Guid? categoryId, Guid? tagId = null,
            BlogPostSortOrder sort = BlogPostSortOrder.Newest,
            bool publishedOnly = false, int page = 1, int pageSize = 20,
            CancellationToken ct = default)
        {
            var posts = await _unitOfWork.BlogPost.GetAllAsync(search, categoryId, tagId, ct);

            IEnumerable<BlogPost> query = posts;
            if (publishedOnly) query = query.Where(p => p.IsPublished);

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

            var pageItems = materialized.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(ToResponse).ToList();

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
            var post = await _unitOfWork.BlogPost.GetByIdAsync(id, ct);
            return post is null ? null : ToResponse(post);
        }

        // ساخت پیش‌نویس: BlogPost + BlogPostContent خالی، هردو Stage میشن،
        // و با یک SaveChangesAsync مشترک با هم Commit میشن (اتمیک).
        public async Task<BlogPostResponse> CreateAsync(CreateBlogPostRequest request, CancellationToken ct = default)
        {
            var post = new BlogPost
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                ReadingMinutes = 0,
                CategoryId = null,
                IsPublished = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            var content = new BlogPostContent
            {
                BlogPostId = post.Id,
                Content = string.Empty
            };

            await _unitOfWork.BlogPost.AddAsync(post, ct);
            await _unitOfWork.BlogPostContent.AddAsync(content, ct);
            await _unitOfWork.SaveChangesAsync(); // یک INSERT-transaction برای هردو ردیف

            var created = await _unitOfWork.BlogPost.GetByIdAsync(post.Id, ct) ?? post;
            return ToResponse(created);
        }

        public async Task<BlogPostResponse?> UpdateAsync(
            Guid id, UpdateBlogPostRequest request, CancellationToken ct = default)
        {
            var post = await _unitOfWork.BlogPost.GetByIdAsync(id, ct);
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
                var uploadResult = await _mediaStorage.SaveAsync(
                    MediaCategory.BlogPostImage, request.CoverImage, entityId,
                    oldFileName: post.CoverImageUrl, ct: ct);
                post.CoverImageUrl = uploadResult.FileName;
            }

            // --- sync tags ---
            var requestedTagIds = (request.TagIds ?? new List<Guid>()).Distinct().ToList();
            var existingTagIds = post.PostTags.Select(pt => pt.BlogTagId).ToHashSet();

            var toRemove = post.PostTags.Where(pt => !requestedTagIds.Contains(pt.BlogTagId)).ToList();
            foreach (var pt in toRemove)
                post.PostTags.Remove(pt);

            foreach (var tagId in requestedTagIds.Where(tid => !existingTagIds.Contains(tid)))
            {
                var newPostTag = new BlogPostTag
                {
                    Id = Guid.NewGuid(),
                    BlogPostId = post.Id,
                    BlogTagId = tagId,
                };
                post.PostTags.Add(newPostTag);
                await _unitOfWork.BlogPost.AddPostTagAsync(newPostTag, ct);
            }

            post.UpdatedAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.BlogPost.GetByIdAsync(post.Id, ct) ?? post;
            return ToResponse(updated);
        }
        public async Task<BlogPostPublishResponse?> TogglePublishAsync(Guid id, CancellationToken ct = default)
        {
            var post = await _unitOfWork.BlogPost.GetByIdAsync(id, ct);
            if (post is null) return null;

            post.IsPublished = !post.IsPublished;
            await _unitOfWork.BlogPost.UpdateAsync(post, ct);
            await _unitOfWork.SaveChangesAsync();

            return new BlogPostPublishResponse(post.Id, post.IsPublished);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var post = await _unitOfWork.BlogPost.GetByIdAsync(id, ct);
            if (post is null) return false;

            await _unitOfWork.BlogPost.DeleteAsync(post, ct);
            await _unitOfWork.SaveChangesAsync();

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
            post.Category?.Title,
            post.PostTags
                .Select(pt => new BlogPostTagResponse(pt.BlogTagId, pt.BlogTag?.Name ?? ""))
                .ToList(),
            post.IsPublished,
            post.CreatedAtUtc,
            post.UpdatedAtUtc,
            post.ViewCount,
            post.LikeCount,
            PersianDateHelper.ToPersianDisplayDate(post.CreatedAtUtc));


        /* --- BLOG CONTENT --- */
        public async Task<BlogPostContentResponse?> GetContentAsync(Guid postId, CancellationToken ct = default)
        {
            var content = await _unitOfWork.BlogPostContent.GetByPostIdAsync(postId, ct);
            return content is null ? null : new BlogPostContentResponse(content.BlogPostId, content.Content);
        }

        public async Task<BlogPostContentResponse?> UpdateContentAsync(
            Guid postId, UpdateBlogPostContentRequest request, CancellationToken ct = default)
        {
            var content = await _unitOfWork.BlogPostContent.GetByPostIdAsync(postId, ct);
            if (content is null) return null; // یعنی پستی با این Id اصلاً وجود نداره (چون هر پست حتماً Content داره)

            content.Content = request.Content;
            await _unitOfWork.BlogPostContent.UpdateAsync(content, ct);
            await _unitOfWork.SaveChangesAsync();

            return new BlogPostContentResponse(content.BlogPostId, content.Content);
        }
    }
}