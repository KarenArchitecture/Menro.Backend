namespace Menro.Domain.Entities.Music.draft
{
    public class CustomerMusicBan
    {
        public Guid Id { get; private set; }

        public Guid RestaurantId { get; private set; }

        public string CustomerIdentifier { get; private set; }

        public DateTime ExpiresAt { get; private set; }
    }
}
