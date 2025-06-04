using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Areas.BusinessOwner.Controllers
{
    [Area("BusinessOwner")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
