using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcDonata.Controllers
{
    public class RingController : Controller
    {
        //
        // GET: /Ring/

        public string Index()
        {
            var app = HttpContext.Application;
            if (app["ring"] != null)
            {
                app["ring"] = null;
                return "true";
            }
            else
            {
                return "";
            }
        }

        public bool CreateRing()
        {
            var app = HttpContext.Application;
            app["ring"] = true;
            return true;
        }

    }
}
