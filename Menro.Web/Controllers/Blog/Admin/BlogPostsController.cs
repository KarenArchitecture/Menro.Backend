using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Exceptions;
using Menro.Application.Common.SD;
using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Blog.Admin
{
    [ApiController]
    [Authorize]
    [Route("api/admin/blog/posts")]
    public class BlogPostsController : ApiControllerBase
    {
        #region DI
        private readonly IBlogPostService _service;
        private readonly IBlogRestaurantSearchService _restaurantSearchService;
        private readonly ICurrentUserService _currentUserService;
        public BlogPostsController(
            IBlogPostService service,
            IBlogRestaurantSearchService restaurantSearchService,
            ICurrentUserService currentUserService)
        {
            _service = service;
            _restaurantSearchService = restaurantSearchService;
            _currentUserService = currentUserService;
        }
        #endregion

        private bool IsElevated() => User.IsInRole(SD.Role_Editor) || User.IsInRole(SD.Role_Admin);
        private bool CanPublish() => User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Editor) || User.IsInRole(SD.Role_Author);


        [HttpGet]
        [Authorize(Roles = SD.Roles_ContributorUp)]
        public async Task<ActionResult<PagedResult<BlogPostAdminListItemResponse>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? categoryId,
            [FromQuery] Guid? tagId,
            [FromQuery] bool onlyMine = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var currentUserId = _currentUserService.GetUserId();
            var result = await _service.GetAllForAdminAsync(
                search, categoryId, tagId, page: page, pageSize: pageSize,
                currentUserId: currentUserId, isElevated: IsElevated(), onlyMine: onlyMine, ct: ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = SD.Roles_ContributorUp)]
        public async Task<ActionResult<BlogPostDetailResponse>> GetById(Guid id, CancellationToken ct)
        {
            var post = await _service.GetByIdAsync(id, _currentUserService.GetUserId(), IsElevated(), ct);
            return post is null ? NotFound() : Ok(post);
        }
        [HttpGet("{slug}")]
        public async Task<ActionResult<BlogPostPublicDetailResponse>> GetBySlug(
            string slug, CancellationToken ct)
        {
            var userId = _currentUserService.GetUserId();
            var post = await _service.GetPublicBySlugAsync(slug, userId, ct);
            if (post is null) return NotFound();
            return Ok(post);
        }

        // POST api/admin/blog/posts - ساخت پیش‌نویس: فقط Title، بدون فایل
        [HttpPost]
        [Authorize(Roles = SD.Roles_ContributorUp)]
        public async Task<ActionResult<BlogPostDetailResponse>> Create(
            [FromBody] CreateBlogPostRequest request, CancellationToken ct)
        {
            var authorId = _currentUserService.GetUserId();
            if (authorId == null)
            {
                return BadRequest();
            }
            var created = await _service.CreateAsync(request, authorId, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/admin/blog/posts/{id}
        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = SD.Roles_ContributorUp)]
        public async Task<ActionResult<BlogPostDetailResponse>> Update(
            Guid id, [FromForm] UpdateBlogPostRequest request, CancellationToken ct)
        {
            try
            {
                var updated = await _service.UpdateAsync(
                    id, request, _currentUserService.GetUserId(), IsElevated(), CanPublish(), ct);
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (BlogPostAccessDeniedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (DuplicateSlugException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPatch("{id:guid}/publish")]
        [Authorize(Roles = SD.Roles_AuthorUp)]
        public async Task<ActionResult<BlogPostPublishResponse>> TogglePublish(Guid id, CancellationToken ct)
        {
            try
            {
                var updated = await _service.TogglePublishAsync(
                    id, _currentUserService.GetUserId(), IsElevated(), ct);
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (BlogPostAccessDeniedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = SD.Roles_AuthorUp)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var deleted = await _service.DeleteAsync(
                    id, _currentUserService.GetUserId(), IsElevated(), ct);
                return deleted ? NoContent() : NotFound();
            }
            catch (BlogPostAccessDeniedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        /* ---------------------------------- */
        /* ---------- BLOG CONTENT ---------- */
        /* ---------------------------------- */
        [HttpGet("{id:guid}/content")]
        [Authorize(Roles = SD.Roles_ContributorUp)]
        public async Task<ActionResult<BlogPostContentResponse>> GetContent(Guid id, CancellationToken ct)
        {
            var content = await _service.GetContentAsync(id, _currentUserService.GetUserId(), IsElevated(), ct);
            return content is null ? NotFound() : Ok(content);
        }

        [HttpPut("{id:guid}/content")]
        [Authorize(Roles = SD.Roles_ContributorUp)]
        public async Task<ActionResult<BlogPostContentResponse>> UpdateContent(
            Guid id, [FromBody] UpdateBlogPostContentRequest request, CancellationToken ct)
        {
            try
            {
                var updated = await _service.UpdateContentAsync(
                    id, request, _currentUserService.GetUserId(), IsElevated(), ct);
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (BlogPostAccessDeniedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        // --- restaurant search for add to blog content
        [HttpGet("restaurant-search")]
        [Authorize(Roles = SD.Roles_ContributorUp)]
        public async Task<ActionResult<IReadOnlyList<BlogRestaurantSearchResult>>> SearchRestaurants(
            [FromQuery] string? term,
            [FromQuery] int take = 10,
            CancellationToken ct = default)
        {
            var results = await _restaurantSearchService.SearchAsync(term, take, ct);
            return Ok(results);
        }

        [HttpGet("restaurant-search/{id:int}")]
        [Authorize(Roles = SD.Roles_ContributorUp)]
        public async Task<ActionResult<BlogRestaurantSearchResult>> GetRestaurant(
            int id, CancellationToken ct)
        {
            var result = await _restaurantSearchService.GetByIdAsync(id, ct);
            return result is null ? NotFound() : Ok(result);
        }

        // --- blog content image upload endpoint
        /* --- CONTENT IMAGES --- */
        [HttpPost("{id:guid}/content/images")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = SD.Roles_ContributorUp)]
        public async Task<ActionResult<BlogContentImageUploadResponse>> UploadContentImage(
            Guid id, IFormFile image, CancellationToken ct)
        {
            try
            {
                var result = await _service.UploadContentImageAsync(
                    id, image, _currentUserService.GetUserId(), IsElevated(), ct);
                return result is null ? NotFound() : Ok(result);
            }
            catch (BlogPostAccessDeniedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

    }
}