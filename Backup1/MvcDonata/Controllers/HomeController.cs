using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcDonata.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var host = Request.Headers["Host"];

            ViewBag.host = host;

            ViewBag.Message = "Welcome to Donata";

            if (host.Contains("127.0.0.1"))
            {
                Response.Redirect("/WebOrder");
            }

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
