namespace Menro.Application.Features.Restaurants.DTOs
{
    public class RestaurantDetailsForAdminDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? LogoImageUrl { get; set; }
        public string Address { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string WorkingHours { get; set; } = "";
        public string CreatedAt { get; set; } = "";

        public string NationalCode { get; set; } = "";
        public string BankAccountNumber { get; set; } = "";
        public string? ShebaNumber { get; set; }

        public string OwnerName { get; set; } = "";
        public string CategoryName { get; set; } = "";

        public double AverageRating { get; set; }
        public int VotersCount { get; set; }

        public int Status { get; set; }
        public string? RejectReason { get; set; }
    }
}