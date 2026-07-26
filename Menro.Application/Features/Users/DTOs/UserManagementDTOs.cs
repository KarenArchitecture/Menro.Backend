namespace Menro.Application.Features.Users.DTOs
{
    public class UserListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UserDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public List<string> Roles { get; set; } = new();
        public int RestaurantsCount { get; set; }
        public int OrdersCount { get; set; }
        public int FavoriteFoodsCount { get; set; }
    }

    public class UserQueryParameters
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UpdateUserRolesDto
    {
        public List<string> Roles { get; set; } = new();
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages =>
            PageSize <= 0 ? 0 : (int)System.Math.Ceiling(TotalCount / (double)PageSize);
    }
}