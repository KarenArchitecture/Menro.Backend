using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Exceptions;
using Menro.Application.Common.Media;
using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services.Interfaces;
using Menro.Application.Helpers;
using Menro.Domain.Entities.Blog;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Blog.Services.Implementations
{
    public class BlogPostService : IBlogPostService
    {
        #region DI
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediaStorageProvider _mediaStorage;

        public BlogPostService(IUnitOfWork unitOfWork, IMediaStorageProvider mediaStorage)
        {
            _unitOfWork = unitOfWork;
            _mediaStorage = mediaStorage;
        }

        #endregion

        // ------------------------------------------------------------
        // Public-facing listing (site): full card DTO, published-only
        // by default, resized cover image.
        // ------------------------------------------------------------
        public async Task<PagedResult<BlogPostListItemResponse>> GetAllAsync(
            string? search, Guid? categoryId, Guid? tagId = null,
            BlogPostSortOrder sort = BlogPostSortOrder.Newest,
            bool publishedOnly = false, int page = 1, int pageSize = 20,
            CancellationToken ct = default)
        {
            var posts = await _unitOfWork.BlogPost.GetAllAsync(search, categoryId, tagId, ct);
            IEnumerable<BlogPost> query = posts;
            if (publishedOnly) query = query.Where(p => p.IsPublished);
            query = ApplySort(query, sort);

            var materialized = query.ToList();
            var totalCount = materialized.Count;
            (page, pageSize) = NormalizePaging(page, pageSize);

            var pageItems = materialized.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(ToListItemResponse).ToList();

            return new PagedResult<BlogPostListItemResponse>
            {
                Items = pageItems,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }

        // ------------------------------------------------------------
        // Admin panel listing: lightweight DTO (thumbnail, author,
        // no body/content-related fields), sees drafts too by default.
        // ------------------------------------------------------------
        public async Task<PagedResult<BlogPostAdminListItemResponse>> GetAllForAdminAsync(
            string? search, Guid? categoryId, Guid? tagId = null,
            BlogPostSortOrder sort = BlogPostSortOrder.Newest,
            int page = 1, int pageSize = 20,
            string? currentUserId = null, bool isElevated = false,
            CancellationToken ct = default)
        {
            var posts = await _unitOfWork.BlogPost.GetAllAsync(search, categoryId, tagId, ct);
            IEnumerable<BlogPost> query = posts;
            if (!isElevated) query = query.Where(p => p.AuthorId == currentUserId);
            query = ApplySort(query, sort);

            var materialized = query.ToList();
            var totalCount = materialized.Count;
            (page, pageSize) = NormalizePaging(page, pageSize);

            var pageItems = materialized.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(ToAdminListItemResponse).ToList();

            return new PagedResult<BlogPostAdminListItemResponse>
            {
                Items = pageItems,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }

        public async Task<BlogPostDetailResponse?> GetByIdAsync(
            Guid id, string? currentUserId = null, bool isElevated = false, CancellationToken ct = default)
        {
            var post = await _unitOfWork.BlogPost.GetByIdAsync(id, ct);
            if (post is null) return null;
            if (!isElevated && post.AuthorId != currentUserId) return null;
            return ToDetailResponse(post);
        }

        // ساخت پیش‌نویس: BlogPost + BlogPostContent خالی، هردو Stage میشن،
        // و با یک SaveChangesAsync مشترک با هم Commit میشن (اتمیک).
        public async Task<BlogPostDetailResponse> CreateAsync(
            CreateBlogPostRequest request, string authorId, CancellationToken ct = default)
        {
            var baseSlug = SlugHelper.GenerateSlug(request.Title);
            var slug = await ResolveUniqueSlugAsync(baseSlug, excludePostId: null, ct);

            var author = await _unitOfWork.User.GetByIdAsync(authorId);
            if (author is null)
                throw new InvalidOperationException($"Authenticated user '{authorId}' was not found.");

            var post = new BlogPost
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Slug = slug,
                AuthorId = authorId,
                AuthorNameSnapshot = author.FullName,
                ReadingMinutes = 0,
                CategoryId = null,
                IsPublished = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            var content = new BlogPostContent { BlogPostId = post.Id, Content = string.Empty };
            await _unitOfWork.BlogPost.AddAsync(post, ct);
            await _unitOfWork.BlogPostContent.AddAsync(content, ct);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.BlogPost.GetByIdAsync(post.Id, ct) ?? post;
            return ToDetailResponse(created);
        }

        public async Task<BlogPostDetailResponse?> UpdateAsync(
            Guid id, UpdateBlogPostRequest request,
            string? currentUserId = null, bool isElevated = false, bool canPublish = false,
            CancellationToken ct = default)
        {
            var post = await _unitOfWork.BlogPost.GetByIdAsync(id, ct);
            if (post is null) return null;

            if (!isElevated && post.AuthorId != currentUserId)
                throw new BlogPostAccessDeniedException();

            post.Title = request.Title.Trim();
            post.ReadingMinutes = request.ReadingMinutes;
            post.CategoryId = request.CategoryId;

            // Contributor هرچی توی IsPublished بفرسته نادیده گرفته میشه - وضعیت
            // فعلی پست دست‌نخورده می‌مونه (نه اجباراً false، نه هرچی خودش خواسته).
            if (canPublish)
                post.IsPublished = request.IsPublished;

            var normalized = SlugHelper.NormalizeAscii(request.Slug);
            if (string.IsNullOrEmpty(normalized))
                normalized = SlugHelper.GenerateSlug(post.Title);
            post.Slug = await ResolveUniqueSlugAsync(normalized, excludePostId: post.Id, ct);

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
            return ToDetailResponse(updated);
        }
        public async Task<BlogPostPublishResponse?> TogglePublishAsync(
            Guid id, string? currentUserId = null, bool isElevated = false, CancellationToken ct = default)
        {
            var post = await _unitOfWork.BlogPost.GetByIdAsync(id, ct);
            if (post is null) return null;
            if (!isElevated && post.AuthorId != currentUserId)
                throw new BlogPostAccessDeniedException();

            post.IsPublished = !post.IsPublished;
            await _unitOfWork.BlogPost.UpdateAsync(post, ct);
            await _unitOfWork.SaveChangesAsync();

            return new BlogPostPublishResponse(post.Id, post.IsPublished);
        }

        public async Task<bool> DeleteAsync(
            Guid id, string? currentUserId = null, bool isElevated = false, CancellationToken ct = default)
        {
            var post = await _unitOfWork.BlogPost.GetByIdAsync(id, ct);
            if (post is null) return false;
            if (!isElevated && post.AuthorId != currentUserId)
                throw new BlogPostAccessDeniedException();

            await _unitOfWork.BlogPost.DeleteAsync(post, ct);
            await _unitOfWork.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(post.CoverImageUrl))
                _mediaStorage.Delete(MediaCategory.BlogPostImage, post.CoverImageUrl, id.ToString());

            return true;
        }

        /* --- BLOG CONTENT --- */
        public async Task<BlogPostContentResponse?> GetContentAsync(
            Guid postId, string? currentUserId = null, bool isElevated = false, CancellationToken ct = default)
        {
            if (!isElevated)
            {
                var owningPost = await _unitOfWork.BlogPost.GetByIdAsync(postId, ct);
                if (owningPost is null) return null;
                if (owningPost.AuthorId != currentUserId) return null;
            }

            var content = await _unitOfWork.BlogPostContent.GetByPostIdAsync(postId, ct);
            return content is null ? null : new BlogPostContentResponse(content.BlogPostId, content.Content);
        }

        public async Task<BlogPostContentResponse?> UpdateContentAsync(
            Guid postId, UpdateBlogPostContentRequest request,
            string? currentUserId = null, bool isElevated = false, CancellationToken ct = default)
        {
            if (!isElevated)
            {
                var owningPost = await _unitOfWork.BlogPost.GetByIdAsync(postId, ct);
                if (owningPost is null) return null;
                if (owningPost.AuthorId != currentUserId)
                    throw new BlogPostAccessDeniedException();
            }

            var content = await _unitOfWork.BlogPostContent.GetByPostIdAsync(postId, ct);
            if (content is null) return null;

            content.Content = request.Content;
            await _unitOfWork.BlogPostContent.UpdateAsync(content, ct);
            await _unitOfWork.SaveChangesAsync();

            return new BlogPostContentResponse(content.BlogPostId, content.Content);
        }

        public async Task<BlogContentImageUploadResponse?> UploadContentImageAsync(
            Guid postId, IFormFile image,
            string? currentUserId = null, bool isElevated = false, CancellationToken ct = default)
        {
            if (!isElevated)
            {
                var owningPost = await _unitOfWork.BlogPost.GetByIdAsync(postId, ct);
                if (owningPost is null) return null;
                if (owningPost.AuthorId != currentUserId)
                    throw new BlogPostAccessDeniedException();
            }

            var content = await _unitOfWork.BlogPostContent.GetByPostIdAsync(postId, ct);
            if (content is null) return null;

            var uploadResult = await _mediaStorage.SaveAsync(
                MediaCategory.BlogContentImage, image, postId.ToString(), oldFileName: null, ct: ct);

            var url = _mediaStorage.GetUrl(
                MediaCategory.BlogContentImage, uploadResult.FileName, postId.ToString(), MediaVariant.Resized);

            return new BlogContentImageUploadResponse(url);
        }


        /* -------   ------------- */
        /* --- Private Helpers --- */
        /* --------   ------------ */
        /// <summary>Appends "-2", "-3", ... on collision - همون الگوی BlogCategoryService.GenerateUniqueSlugAsync.</summary>
        private async Task<string> ResolveUniqueSlugAsync(string desiredSlug, Guid? excludePostId, CancellationToken ct)
        {
            var baseSlug = desiredSlug;
            var slug = baseSlug;
            var suffix = 2;
            while (await _unitOfWork.BlogPost.SlugExistsAsync(slug, excludePostId, ct))
            {
                slug = $"{baseSlug}-{suffix}";
                suffix++;
            }
            return slug;
        }

        private static IEnumerable<BlogPost> ApplySort(IEnumerable<BlogPost> query, BlogPostSortOrder sort) =>
            sort switch
            {
                BlogPostSortOrder.MostPopular => query.OrderByDescending(p => p.LikeCount),
                BlogPostSortOrder.MostViewed => query.OrderByDescending(p => p.ViewCount),
                _ => query.OrderByDescending(p => p.CreatedAtUtc),
            };

        private static (int page, int pageSize) NormalizePaging(int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);
            return (page, pageSize);
        }

        private BlogPostListItemResponse ToListItemResponse(BlogPost post) => new(
            post.Id,
            post.Title,
            post.Slug,
            string.IsNullOrWhiteSpace(post.CoverImageUrl)
                ? null
                : _mediaStorage.GetUrl(MediaCategory.BlogPostImage, post.CoverImageUrl, post.Id.ToString(), MediaVariant.Resized),
            post.ReadingMinutes,
            post.CategoryId,
            post.Category?.Title,
            post.IsPublished,
            post.CreatedAtUtc,
            post.UpdatedAtUtc,
            post.ViewCount,
            post.LikeCount,
            PersianDateHelper.ToPersianDisplayDate(post.CreatedAtUtc));

        private BlogPostAdminListItemResponse ToAdminListItemResponse(BlogPost post) => new(
            post.Id,
            post.Title,
            string.IsNullOrWhiteSpace(post.CoverImageUrl)
                ? null
                : _mediaStorage.GetUrl(MediaCategory.BlogPostImage, post.CoverImageUrl, post.Id.ToString(), MediaVariant.Thumbnail),
            post.ReadingMinutes,
            post.CategoryId,
            post.Category?.Title,
            post.Author?.FullName ?? post.AuthorNameSnapshot,
            post.IsPublished,
            post.CreatedAtUtc,
            PersianDateHelper.ToPersianDisplayDate(post.CreatedAtUtc));

        private BlogPostDetailResponse ToDetailResponse(BlogPost post) => new(
            post.Id,
            post.Title,
            post.Slug,
            string.IsNullOrWhiteSpace(post.CoverImageUrl)
                ? null
                : _mediaStorage.GetUrl(MediaCategory.BlogPostImage, post.CoverImageUrl, post.Id.ToString(), MediaVariant.Original),
            post.AuthorId,
            AuthorName: post.Author?.FullName ?? post.AuthorNameSnapshot,
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

    }
}