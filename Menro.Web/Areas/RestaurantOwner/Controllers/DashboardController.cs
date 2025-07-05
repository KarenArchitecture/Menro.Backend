using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Menro.Application.Common.SD;
using Menro.Application.DTO;
using Menro.Web.Areas.RestaurantOwner.ViewModels;
using Menro.Application.Restaurants.Services.Interfaces;

namespace Menro.Web.Areas.RestaurantOwner.Controllers
{
    [Area("RestaurantOwner")]
    //[Authorize(Roles = SD.Role_Owner)]
    public class DashboardController : Controller
    {
        private readonly IRestaurantService _restaurantService;
        public DashboardController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        //public IActionResult Index()
        //{
        //    RestaurantDashboardVM vm = _restaurantService.
        //    return View(vm);
        //}
    }
}
