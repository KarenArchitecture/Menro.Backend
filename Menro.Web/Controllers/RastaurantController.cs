using Menro.Application.Services.Interfaces;
using Menro.Application.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers
{
    public class RastaurantController : Controller
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IRestaurantCategoryService _restaurantCategoryService;
        public RastaurantController (IRestaurantService restaurantService, IRestaurantCategoryService restaurantCategoryService)
        {
            _restaurantService = restaurantService;
            _restaurantCategoryService = restaurantCategoryService;
        }

        // GET: RastaurantController
        public ActionResult Index()
        {
            return View();
        }

        // GET: RastaurantController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RastaurantController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RastaurantController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RestaurantDto dto)
        {
            bool result = await _restaurantService.AddRestaurantAsync(dto);

            if (result)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(dto);
            }
        }
        // GET: RastaurantController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: RastaurantController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: RastaurantController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: RastaurantController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
