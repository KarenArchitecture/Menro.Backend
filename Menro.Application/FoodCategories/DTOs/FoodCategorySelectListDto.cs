
namespace Menro.Application.FoodCategories.DTOs
{
    public class FoodCategorySelectListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? GlobalCategoryId { get; set; } = null;
    }

}
