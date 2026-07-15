namespace Menro.Application.Features.Foods.DTOs
{
    public class PopularFoodsDto
    {
        public int CategoryId { get; set; }
        public string CategoryTitle { get; set; } = string.Empty;
        public int? IconId { get; set; } = 0;
        public List<HomeFoodCardDto> Foods { get; set; } = new();
    }
}
