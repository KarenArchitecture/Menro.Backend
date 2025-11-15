namespace Menro.Application.Foods.DTOs
{
    public class FoodsListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public bool? IsAvailable { get; set; } = true; 
        public string FoodCategoryName { get; set; } = string.Empty;
    }
}
