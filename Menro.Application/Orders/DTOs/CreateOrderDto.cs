using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Orders.DTOs
{
    /// <summary>
    /// Main DTO used by the frontend to create an order.
    /// Contains restaurant info, table code, and the list of items.
    /// </summary>
    public class CreateOrderDto
    {
        /// <summary>
        /// Restaurant for which the order is placed (Restaurant.Id).
        /// </summary>
        [Required]
        public int RestaurantId { get; set; }

        /// <summary>
        /// Selected table code (e.g. "t3", "t5", "takeout"), or null.
        /// This comes from the table selector in the checkout footer.
        /// </summary>
        public string? TableCode { get; set; }

        /// <summary>
        /// Line items included in the order.
        /// </summary>
        [MinLength(1, ErrorMessage = "An order must contain at least one item.")]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
