using Menro.Application.Common.SD;
using Menro.Application.Features.Landing.DTOs;
using Menro.Application.Features.Landing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Landing
{
    /// <summary>
    /// Admin-only endpoints backing LandingManagementSection.jsx:
    ///   - /general        -> hero image + hero texts + "با منرو تو چشم باش" heading
    ///   - /reasons        -> "چرا منرو؟" cards
    ///   - /faqs           -> "سوالات متداول"
    ///
    /// Intentionally NOT covered here (per product decision - see the info
    /// banner in LandingManagementSection.jsx):
    ///   - hero stats (computed live, not editable content)
    ///   - subscription plan cards (depend on the subscription system)
    ///   - blog cards on the landing page (managed from the Blog admin tab)
    /// </summary>
    [ApiController]
    [Authorize(Roles = SD.Role_Admin)]
    [Route("api/admin/landing")]
    public class AdminLandingController : ControllerBase
    {
        private readonly ILandingGeneralService _generalService;
        private readonly ILandingReasonService _reasonService;
        private readonly ILandingFaqService _faqService;

        public AdminLandingController(
            ILandingGeneralService generalService,
            ILandingReasonService reasonService,
            ILandingFaqService faqService)
        {
            _generalService = generalService;
            _reasonService = reasonService;
            _faqService = faqService;
        }

        /* ============================================================ */
        /* GENERAL                                                       */
        /* ============================================================ */

        [HttpGet("general")]
        public async Task<ActionResult<LandingGeneralResponse>> GetGeneral()
        {
            var result = await _generalService.GetAsync();
            return Ok(result);
        }

        [HttpPut("general")]
        public async Task<ActionResult<LandingGeneralResponse>> UpdateGeneral(
            [FromBody] UpdateLandingGeneralRequest request)
        {
            var result = await _generalService.UpdateAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Uploads a new hero image. Pass the currently-stored file name as
        /// <paramref name="oldFileName"/> when replacing an existing image so
        /// the old file gets cleaned up from disk - same convention as
        /// POST /api/admin/blog/posts/cover-image.
        /// </summary>
        [HttpPost("general/hero-image")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
        public async Task<ActionResult<UploadLandingHeroImageResponse>> UploadHeroImage(
            IFormFile file,
            [FromQuery] string? oldFileName)
        {
            if (file is null || file.Length == 0)
                return BadRequest("فایلی برای آپلود ارسال نشده است.");

            var result = await _generalService.UploadHeroImageAsync(file, oldFileName);
            return Ok(result);
        }

        /* ============================================================ */
        /* REASONS ("چرا منرو؟")                                          */
        /* ============================================================ */

        [HttpGet("reasons")]
        public async Task<ActionResult<List<LandingReasonResponse>>> GetReasons()
        {
            var result = await _reasonService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost("reasons")]
        public async Task<ActionResult<LandingReasonResponse>> CreateReason(
            [FromBody] CreateLandingReasonRequest request)
        {
            var created = await _reasonService.CreateAsync(request);
            return CreatedAtAction(nameof(GetReasons), new { id = created.Id }, created);
        }

        [HttpPut("reasons/{id:guid}")]
        public async Task<ActionResult<LandingReasonResponse>> UpdateReason(
            Guid id,
            [FromBody] UpdateLandingReasonRequest request)
        {
            var updated = await _reasonService.UpdateAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("reasons/{id:guid}")]
        public async Task<IActionResult> DeleteReason(Guid id)
        {
            await _reasonService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>direction query param must be "up" or "down" (matches the admin UI's move buttons).</summary>
        [HttpPut("reasons/{id:guid}/move")]
        public async Task<IActionResult> MoveReason(Guid id, [FromQuery] string direction)
        {
            await _reasonService.MoveAsync(id, direction);
            return NoContent();
        }

        /* ============================================================ */
        /* FAQ ("سوالات متداول")                                          */
        /* ============================================================ */

        [HttpGet("faqs")]
        public async Task<ActionResult<List<LandingFaqResponse>>> GetFaqs()
        {
            var result = await _faqService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost("faqs")]
        public async Task<ActionResult<LandingFaqResponse>> CreateFaq(
            [FromBody] CreateLandingFaqRequest request)
        {
            var created = await _faqService.CreateAsync(request);
            return CreatedAtAction(nameof(GetFaqs), new { id = created.Id }, created);
        }

        [HttpPut("faqs/{id:guid}")]
        public async Task<ActionResult<LandingFaqResponse>> UpdateFaq(
            Guid id,
            [FromBody] UpdateLandingFaqRequest request)
        {
            var updated = await _faqService.UpdateAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("faqs/{id:guid}")]
        public async Task<IActionResult> DeleteFaq(Guid id)
        {
            await _faqService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>direction query param must be "up" or "down" (matches the admin UI's move buttons).</summary>
        [HttpPut("faqs/{id:guid}/move")]
        public async Task<IActionResult> MoveFaq(Guid id, [FromQuery] string direction)
        {
            await _faqService.MoveAsync(id, direction);
            return NoContent();
        }
    }
}
