namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantDetailsForAdminDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Address { get; set; } = "";
        public string Type { get; set; } = "";
        public string WorkingHours { get; set; } = "";

        public string OwnerNationalId { get; set; } = "";
        public string OwnerBankAccount { get; set; } = "";
        public string OwnerPhoneNumber { get; set; } = "";
        public string OwnerName { get; set; } = "";

        public int Status { get; set; }

        public string CreatedAt { get; set; } = "";

        public string? RejectReason { get; set; } = "";
    }
}
