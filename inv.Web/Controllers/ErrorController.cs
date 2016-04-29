using System.Web.Mvc;

namespace Invest.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.StatusCode = 500;

            if (TempData["message"] != null)
                ViewBag.ErrorMessage = TempData["message"];

            return View("Error");
        }

        public ActionResult PageNotFound()
        {
            //ToDo: Log Error

            ViewBag.StatusCode = 404;
            return View("Error");
        }

        public ActionResult AccessDenied()
        {
            ViewBag.StatusCode = 403;
            return View("Error");
        }
    }
}
