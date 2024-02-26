using Microsoft.AspNetCore.Mvc;

namespace Alloc8_web.Controllers
{
    public class testController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
