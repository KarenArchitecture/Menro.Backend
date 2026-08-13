namespace Menro.Application.Features.Foods.DTOs
{
    public class PopularFoodsDto
    {
        public int CategoryId { get; set; }
        public string CategoryTitle { get; set; } = string.Empty;
        public string SvgIcon { get; set; } = string.Empty;
        public List<HomeFoodCardDto> Foods { get; set; } = new();
    }
}
