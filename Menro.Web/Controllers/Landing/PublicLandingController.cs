using Menro.Application.Features.Landing.DTOs;
using Menro.Application.Features.Landing.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Landing
{
    /// <summary>
    /// Public, read-only endpoints backing the public landing page
    /// (LandingPage.jsx and its children: Hero, WhyMenroSection,
    /// BurgerPanelSection, FAQSection).
    ///
    /// No [Authorize] here on purpose — this is the marketing homepage,
    /// anyone can hit it. It reuses the exact same
    /// ILandingGeneralService / ILandingReasonService / ILandingFaqService
    /// as AdminLandingController; there's no separate "public" business
    /// logic, just a read-only surface over the same content, same as how
    /// public blog reads reuse the blog services.
    ///
    /// Intentionally NOT covered here (per product decision — see the info
    /// banner in LandingManagementSection.jsx / AdminLandingController):
    ///   - hero stats (computed live, not editable/stored content)
    ///   - subscription plan cards (depend on the subscription system)
    ///   - blog cards on the landing page (served by the public Blog API)
    /// </summary>
    [ApiController]
    [Route("api/landing")]
    public class PublicLandingController : ControllerBase
    {
        private readonly ILandingGeneralService _generalService;
        private readonly ILandingReasonService _reasonService;
        private readonly ILandingFaqService _faqService;

        public PublicLandingController(
            ILandingGeneralService generalService,
            ILandingReasonService reasonService,
            ILandingFaqService faqService)
        {
            _generalService = generalService;
            _reasonService = reasonService;
            _faqService = faqService;
        }

        /// <summary>Hero image + hero texts + "با منرو تو چشم باش" heading text.</summary>
        [HttpGet("general")]
        public async Task<ActionResult<LandingGeneralResponse>> GetGeneral()
        {
            var result = await _generalService.GetAsync();
            return Ok(result);
        }

        /// <summary>"چرا منرو؟" cards, already ordered by SortOrder.</summary>
        [HttpGet("reasons")]
        public async Task<ActionResult<List<LandingReasonResponse>>> GetReasons()
        {
            var result = await _reasonService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>"سوالات متداول", already ordered by SortOrder.</summary>
        [HttpGet("faqs")]
        public async Task<ActionResult<List<LandingFaqResponse>>> GetFaqs()
        {
            var result = await _faqService.GetAllAsync();
            return Ok(result);
        }
    }
}
