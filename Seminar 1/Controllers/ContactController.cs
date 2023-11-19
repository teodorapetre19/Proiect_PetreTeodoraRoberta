using Microsoft.AspNetCore.Mvc;

namespace Seminar_1.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
