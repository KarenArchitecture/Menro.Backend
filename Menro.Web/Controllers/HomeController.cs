using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
