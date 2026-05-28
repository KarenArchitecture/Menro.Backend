using Menro.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using Menro.Domain.Interfaces.Persistence;

namespace Menro.Domain.Entities
{
    public class Discount : ISoftDeletable
    {
        public int Id { get; set; }

        public DiscountScope Scope { get; set; }

        public int? RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }

        public int? FoodId { get; set; }
        public Food? Food { get; set; }

        public DiscountValueType ValueType { get; set; }
        public decimal Value { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        // ✅ Soft delete OK
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}