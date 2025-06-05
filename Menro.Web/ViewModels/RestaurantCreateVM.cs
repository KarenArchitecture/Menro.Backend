using Menro.Application.DTO;
using System.ComponentModel.DataAnnotations;

namespace Menro.Web.ViewModels
{
    public class RestaurantCreateVM
    {
        public RestaurantDto Restaurant { get; set; } = new RestaurantDto();
        public List<RestaurantCategoryDto> Categories { get; set; } = new List<RestaurantCategoryDto>();
    }
}