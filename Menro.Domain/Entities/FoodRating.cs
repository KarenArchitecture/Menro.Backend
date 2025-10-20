using System.ComponentModel.DataAnnotations;


namespace Menro.Domain.Entities
{
    public class FoodRating
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 5)]
        public int Score { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK to Food
        [Required]
        public int FoodId { get; set; }
        public Food Food { get; set; }

        // FK to User
        [Required]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; }
    }
}
