using System.ComponentModel.DataAnnotations;

namespace Menro.Domain.Entities;

public class FavoriteFood
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    [Required]
    public int FoodId { get; set; }

    public Food Food { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}