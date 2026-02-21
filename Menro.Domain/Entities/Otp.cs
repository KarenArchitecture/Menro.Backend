namespace Menro.Domain.Entities
{
    public class Otp
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsUsed { get; set; }
    }
}
