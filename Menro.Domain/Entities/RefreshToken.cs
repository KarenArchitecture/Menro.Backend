namespace Menro.Domain.Entities.Identity
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string UserId { get; set; } = default!;
        public string TokenHash { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CreatedByIp { get; set; } = default!;
        public string? UserAgent { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }
    }
}
