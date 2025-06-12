using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Areas.BusinessOwner.Controllers
{
    public class TempController : Controller
    {
        [Area("BusinessOwner")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
