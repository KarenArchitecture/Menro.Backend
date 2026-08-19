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

        // get posts for public Blog feed page
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


        // get posts list for admin page based on role and access level
        public async Task<PagedResult<BlogPostAdminListItemResponse>> GetAllForAdminAsync(
            string? search, Guid? categoryId, Guid? tagId = null,
            BlogPostSortOrder sort = BlogPostSortOrder.Newest,
            int page = 1, int pageSize = 20,
            string? currentUserId = null, bool isElevated = false, bool onlyMine = false,
            CancellationToken ct = default)
        {
            var posts = await _unitOfWork.BlogPost.GetAllAsync(search, categoryId, tagId, ct);
            IEnumerable<BlogPost> query = posts;

            if (!isElevated || onlyMine)
                query = query.Where(p => p.AuthorId == currentUserId);

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

        // get post by Id
        public async Task<BlogPostDetailResponse?> GetByIdAsync(
            Guid id, string? currentUserId = null, bool isElevated = false, CancellationToken ct = default)
        {
            var post = await _unitOfWork.BlogPost.GetByIdAsync(id, ct);
            if (post is null) return null;
            if (!isElevated && post.AuthorId != currentUserId) return null;
            return ToDetailResponse(post);
        }

        // related posts by tag in public blog post page
        public async Task<IReadOnlyList<BlogPostRelatedItemResponse>> GetRelatedPostsAsync(
            string slug, int count = 4, CancellationToken ct = default)
        {
            var current = await _unitOfWork.BlogPost.GetBySlugAsync(slug, ct);
            if (current is null || !current.IsPublished)
                return Array.Empty<BlogPostRelatedItemResponse>();

            var candidates = (await _unitOfWork.BlogPost.GetPublishedWithTagsAsync(ct))
                .Where(p => p.Id != current.Id)
                .ToList();

            var currentTagIds = current.PostTags.Select(pt => pt.BlogTagId).ToHashSet();
            var result = new List<BlogPost>();
            var usedIds = new HashSet<Guid>();

            // 1) shared tags, most overlap first, then newest
            if (currentTagIds.Count > 0)
            {
                var byTag = candidates
                    .Select(p => new
                    {
                        Post = p,
                        Overlap = p.PostTags.Count(pt => currentTagIds.Contains(pt.BlogTagId))
                    })
                    .Where(x => x.Overlap > 0)
                    .OrderByDescending(x => x.Overlap)
                    .ThenByDescending(x => x.Post.CreatedAtUtc)
                    .Select(x => x.Post);

                foreach (var p in byTag)
                {
                    if (result.Count >= count) break;
                    if (usedIds.Add(p.Id)) result.Add(p);
                }
            }

            // 2) fallback: same category, newest first
            if (result.Count < count && current.CategoryId.HasValue)
            {
                var byCategory = candidates
                    .Where(p => p.CategoryId == current.CategoryId && !usedIds.Contains(p.Id))
                    .OrderByDescending(p => p.CreatedAtUtc);

                foreach (var p in byCategory)
                {
                    if (result.Count >= count) break;
                    if (usedIds.Add(p.Id)) result.Add(p);
                }
            }

            // 3) fallback: newest overall
            if (result.Count < count)
            {
                var newest = candidates
                    .Where(p => !usedIds.Contains(p.Id))
                    .OrderByDescending(p => p.CreatedAtUtc);

                foreach (var p in newest)
                {
                    if (result.Count >= count) break;
                    if (usedIds.Add(p.Id)) result.Add(p);
                }
            }

            return result.Select(ToRelatedItemResponse).ToList();
        }

        // popular posts in blog post page
        public async Task<IReadOnlyList<BlogPostRelatedItemResponse>> GetPopularPostsAsync(
            string slug, int count = 5, CancellationToken ct = default)
        {
            var posts = await _unitOfWork.BlogPost.GetAllAsync(null, null, null, ct);

            return posts
                .Where(p => p.IsPublished && p.Slug != slug)
                .OrderByDescending(p => p.ViewCount)
                .Take(count)
                .Select(ToRelatedItemResponse)
                .ToList();
        }

        // for public blog post page
        public async Task<BlogPostPublicDetailResponse?> GetPublicBySlugAsync(
            string slug, string? currentUserId = null, CancellationToken ct = default)
        {
            var post = await _unitOfWork.BlogPost.GetBySlugAsync(slug, ct);
            if (post is null || !post.IsPublished) return null;

            var isLiked = currentUserId is not null
                && await _unitOfWork.BlogPostLike.ExistsAsync(post.Id, currentUserId, ct);

            return ToPublicDetailResponse(post, isLiked);
        }

        // create post with empty content, un-published
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

        // update blog post
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
            post.Slug = await EnsureUniqueSlugForUpdateAsync(normalized, post.Id, ct);

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

        // publish/draft blog post
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

        // delete blog post
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

        public async Task TrackViewAsync(string slug, string visitorHash, CancellationToken ct = default)
        {
            await _unitOfWork.BlogPost.IncrementViewCountIfNotSeenAsync(slug, visitorHash, ct);
        }

        public async Task<BlogPostLikeResponse?> ToggleLikeAsync(
            string slug, string userId, CancellationToken ct = default)
        {
            var post = await _unitOfWork.BlogPost.GetBySlugAsync(slug, ct);
            if (post is null || !post.IsPublished) return null;

            var alreadyLiked = await _unitOfWork.BlogPostLike.ExistsAsync(post.Id, userId, ct);

            if (alreadyLiked)
            {
                await _unitOfWork.BlogPostLike.RemoveAsync(post.Id, userId, ct);
                post.LikeCount = Math.Max(0, post.LikeCount - 1);
            }
            else
            {
                await _unitOfWork.BlogPostLike.AddAsync(new BlogPostLike
                {
                    Id = Guid.NewGuid(),
                    BlogPostId = post.Id,
                    UserId = userId,
                    CreatedAtUtc = DateTime.UtcNow,
                }, ct);
                post.LikeCount++;
            }

            await _unitOfWork.BlogPost.UpdateAsync(post, ct);
            await _unitOfWork.SaveChangesAsync();

            return new BlogPostLikeResponse(!alreadyLiked, post.LikeCount);
        }

        /* --- BLOG CONTENT --- */

        // get content
        public async Task<BlogPostContentResponse?> GetContentAsync(
            Guid postId, string? currentUserId = null, bool isElevated = false, CancellationToken ct = default)
        {
            // همیشه پست رو می‌گیریم (نه فقط توی حالت !isElevated) - چون هم برای
            // چک مالکیت لازمه، هم برای اینکه مطمئن بشیم اصلاً پستی با این Id
            // وجود داره یا نه (قبل از این‌که تصمیم بگیریم Content گمشده‌ش رو بسازیم).
            var owningPost = await _unitOfWork.BlogPost.GetByIdAsync(postId, ct);
            if (owningPost is null) return null; // خودِ پست وجود نداره

            if (!isElevated && owningPost.AuthorId != currentUserId)
                return null;

            var content = await _unitOfWork.BlogPostContent.GetByPostIdAsync(postId, ct);
            if (content is null)
            {
                // پست هست ولی ردیف BlogPostContent ش گم شده (مثلاً دیتای قدیمی از
                // قبل اصلاح رابطه‌ی ۱-۱، یا هر دلیل دیگه‌ای که از سینک خارج شده).
                // به‌جای مجبورکردن کاربر به حذف کل پست، همین‌جا Content خالی
                // رو دوباره می‌سازیم تا ویرایش عادی ادامه پیدا کنه.
                content = new BlogPostContent { BlogPostId = postId, Content = string.Empty };
                await _unitOfWork.BlogPostContent.AddAsync(content, ct);
                await _unitOfWork.SaveChangesAsync();
            }

            return new BlogPostContentResponse(content.BlogPostId, content.Content);
        }

        // update content
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

        // upload content image
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

        private async Task<string> EnsureUniqueSlugForUpdateAsync(string desiredSlug, Guid excludePostId, CancellationToken ct)
        {
            if (await _unitOfWork.BlogPost.SlugExistsAsync(desiredSlug, excludePostId, ct))
                throw new DuplicateSlugException();
            return desiredSlug;
        }
        /* --- MAPPERS --- */

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
            post.AuthorId,
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

        private BlogPostPublicDetailResponse ToPublicDetailResponse(BlogPost post, bool isLiked) => new(
            post.Id,
            post.Title,
            post.Slug,
            string.IsNullOrWhiteSpace(post.CoverImageUrl)
                ? null
                : _mediaStorage.GetUrl(MediaCategory.BlogPostImage, post.CoverImageUrl, post.Id.ToString(), MediaVariant.Original),
            post.Content?.Content ?? string.Empty,
            post.Author?.FullName ?? post.AuthorNameSnapshot,
            post.CategoryId,
            post.Category?.Title,
            post.Category?.Slug,
            post.PostTags
                .Select(pt => new BlogPostPublicTagResponse(
                    pt.BlogTagId,
                    pt.BlogTag?.Name ?? "",
                    pt.BlogTag?.Slug ?? ""))
                .ToList(),
            post.ReadingMinutes,
            post.ViewCount,
            post.LikeCount,
            PersianDateHelper.ToPersianDisplayDate(post.CreatedAtUtc),
            isLiked);

        private BlogPostRelatedItemResponse ToRelatedItemResponse(BlogPost post) => new(
            post.Id,
            post.Slug,
            post.Title,
            string.IsNullOrWhiteSpace(post.CoverImageUrl)
                ? null
                : _mediaStorage.GetUrl(MediaCategory.BlogPostImage, post.CoverImageUrl, post.Id.ToString(), MediaVariant.Thumbnail),
            post.ReadingMinutes);

    }
}