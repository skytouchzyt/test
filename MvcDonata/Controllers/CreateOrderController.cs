using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcDonata.Controllers
{
    /// <summary>
    /// 创建订单:网络订单,电话订单,堂食订单
    /// </summary>
    public class CreateOrderController : Controller
    {
        //
        // GET: /CreateOrder/

        public ActionResult Index(string phone)
        {
            return View();
        }

    }
}
