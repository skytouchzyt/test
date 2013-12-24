using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcDonata.Controllers
{
    public class ReportsController : Controller
    {
        //
        // GET: /Reports/

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult GetNodesByOrders(DateTime date)
        {
            return Json(Models.BusinessObject.GetNodesByOrders(date), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult GetWeeklyData(DateTime date)
        {
            return Json(Models.BusinessObject.GetWeeklyData(date), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult GetMonthlyData(DateTime date)
        {
            return Json(Models.BusinessObject.GetMonthlyData(date), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult GetBusyStatus(DateTime date)
        {
            return Json(Models.BusinessObject.GetBusyStatus(date), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Test()
        {
            return View();
        }
    }
}
