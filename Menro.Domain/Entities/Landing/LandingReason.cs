using System;

namespace Menro.Domain.Entities.Landing
{
    /// <summary>
    /// One card in the "چرا منرو؟" section of the landing page.
    /// </summary>
    public class LandingReason
    {
        public Guid Id { get; set; }

        /// <summary>Font Awesome icon class, e.g. "fas fa-headphones-simple".</summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>Hex color used for the icon badge, e.g. "#7C3AED".</summary>
        public string ColorHex { get; set; } = "#F59E0B";

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Zero-based display order. Adjacent items are swapped by the
        /// admin "move up / move down" actions.
        /// </summary>
        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
