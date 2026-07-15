using System;

namespace Menro.Domain.Entities.Landing
{
    /// <summary>
    /// One question/answer pair in the landing page's "سوالات متداول" section.
    /// </summary>
    public class LandingFaq
    {
        public Guid Id { get; set; }

        public string Question { get; set; } = string.Empty;

        /// <summary>Can be long - rendered as free text (no HTML) on the public page.</summary>
        public string Answer { get; set; } = string.Empty;

        /// <summary>
        /// Zero-based display order. Adjacent items are swapped by the
        /// admin "move up / move down" actions.
        /// </summary>
        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
