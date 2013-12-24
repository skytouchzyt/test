using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcDonata.Controllers
{
    public class ProductController : Controller
    {
        //
        // GET: /Product/
        [Authorize(Roles="店长")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "管理员")]
        public ActionResult AddOrder()
        {
            return View();
        }


    }
}
