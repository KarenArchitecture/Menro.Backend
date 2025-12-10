using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Orders.DTOs
{
    /// <summary>
    /// Represents a single line item in an order:
    /// one food, optional variant, quantity, and selected addons.
    /// </summary>
    public class CreateOrderItemDto
    {
        /// <summary>
        /// Food ID (Food.Id).
        /// For simple foods (without variants) this is required.
        /// If VariantId is provided, FoodId can be null and will be inferred from the variant.
        /// </summary>
        public int? FoodId { get; set; }

        /// <summary>
        /// Selected variant ID (FoodVariant.Id), if the food has variants.
        /// Null for simple foods without variants.
        /// </summary>
        public int? VariantId { get; set; }

        /// <summary>
        /// Quantity of this item in the order.
        /// Example: 2 × “Kebab – Large”.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        /// <summary>
        /// Snapshot unit price at the moment of ordering.
        /// This is what the frontend calculates (variant price + addons).
        /// Backend will recalculate and compare to prevent tampering.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Selected addon IDs (FoodAddon.Id values).
        /// The list can be empty if no addons were selected.
        /// </summary>
        public List<int> ExtraIds { get; set; } = new();
    }
}
