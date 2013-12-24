using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
using MvcDonata.Models;

namespace MvcDonata.Controllers
{
    public class StoreController : Controller
    {
        //
        // GET: /Store/
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Order()
        {
            ViewBag.Node = DonataMembership.GetUserNode(User.Identity.Name);
            return View();
        }

        /// <summary>
        /// 保存订货单,为了从总库房拿货
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public ActionResult SaveOrder(string jsonData)
        {
            try
            {
                var details = jsonData.DeserializeFromJson<List<StoreDetailModal>>(false);
                var node = DonataMembership.GetUserNode(User.Identity.Name);
                StoreBusinessObject.InsertOrder(User.Identity.Name,node, "订货单", "",DateTime.Now, details);
                return Json(new iJsonResult("success"));
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message));
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult SaveStockIn(string provider,string datas,DateTime time)
        {
            try
            {
                var details = datas.DeserializeFromJson<List<StoreDetailModal>>(false);
                var node = DonataMembership.GetUserNode(User.Identity.Name);
                StoreBusinessObject.InsertOrder(User.Identity.Name, node, "进货单",new { Provider = provider }.SerializeToJson(false),time, details);
                return Json(new iJsonResult("success"));
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message));
            }
            
        }
    

        public ActionResult GetRecentProducts()
        {
            try
            {
                return Json(StoreBusinessObject.GetRecentProducts(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 获取各分店一周订货单
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResult GetNodeOrders(DateTime start,string orderType)
        {
            try
            {
               

                var node=DonataMembership.GetUserNode(User.Identity.Name);
                if (node == "总库房")
                    node = "全部";
                return Json(StoreBusinessObject.GetNodeOrdersList(node,orderType,start,start+new TimeSpan(14,0,0,0)),JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult GetMonthNodeOrders(DateTime time, string orderType,string provider="全部",string node="全部",bool combined=false)
        {
            try
            {
                var firstDay = new DateTime(time.Year, time.Month, 1);
                var lastDay = new DateTime(time.Year, time.Month, DateTime.DaysInMonth(time.Year, time.Month))+new TimeSpan(1,0,0,0);

                var userNode=DonataMembership.GetUserNode(User.Identity.Name);
                if (userNode != "总库房")
                    node = userNode;
                return Json(StoreBusinessObject.GetNodeOrdersList(node, orderType,firstDay, lastDay,provider,combined), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetNodeOrderDetails(string datas,bool usedOrderPrice=true)
        {
            try
            {
                var list = datas.DeserializeFromJson<List<int>>(false);
                return Json(StoreBusinessObject.GetNodeOrderDetails(list,usedOrderPrice), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }


        [Authorize]
        [HttpPost]
        public ActionResult DeleteNodeOrder(int orderID)
        {
            try
            {
                StoreBusinessObject.DeleteNodeOrder(orderID);
                return Json(new iJsonResult("success"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "库管")]
        [HttpPost]
        public ActionResult StockOut(string datas,string stockoutNode="")
        {
            var ids=datas.DeserializeFromJson<List<int>>(false);
            ViewBag.orderIDs = datas;
            if (string.IsNullOrEmpty(stockoutNode))
                ViewBag.node = StoreBusinessObject.GetNodeByOrderID(ids[0]);
            else
                ViewBag.node = stockoutNode;
            
            return View();
        }

        [Authorize(Roles = "库管")]
        [HttpPost]
        public ActionResult SaveStockOut(string node,string datas)
        {
            try
            {
                var details = datas.DeserializeFromJson<List<StoreDetailModal>>(false);
                StoreBusinessObject.InsertOrder(User.Identity.Name, node, "出库单", "",DateTime.Now, details);
                return Json(new iJsonResult("success"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }


        //[Authorize]
        public ActionResult GetAllProducts()
        {
            try
            {
                return Json(StoreBusinessObject.GetProducts(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateCommand(string node,string command)
        {
            try
            {
                if (string.IsNullOrEmpty(node))
                    node = DonataMembership.GetUserNode(User.Identity.Name);
                StoreBusinessObject.InsertCommand(node, User.Identity.Name, command);
                return Json(new iJsonResult("success"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
            
        }

        public ActionResult GetCommands(string node)
        {
            try
            {
                return Json(StoreBusinessObject.GetCommands(node), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult StockIn()
        {
            ViewBag.Node = DonataMembership.GetUserNode(User.Identity.Name);
            return View();
        }


        [Authorize]
        public ActionResult GetProviders()
        {
            try
            {
                return Json(StoreBusinessObject.GetProviders(DonataMembership.GetUserNode(User.Identity.Name)), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult GetNodes()
        {
            try
            {
                return Json(StoreBusinessObject.GetNodes(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult GetProductsByProvider(string provider)
        {
            try
            {
                return Json(StoreBusinessObject.GetProducts(provider), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 生成东郊采购订单
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public ActionResult EastMarkOrder(string datas)
        {
            ViewBag.Orders = datas;
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetEastMarkOrderDetails(string datas)
        {
            try
            {
                var ids = datas.DeserializeFromJson<List<int>>(false);
                return Json(StoreBusinessObject.GetEastMarkOrderDetails(ids), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
            
        }

        [HttpPost]
        [Authorize(Roles = "库管")]
        public ActionResult ViewStock(string datas)
        {
            ViewBag.datas = datas;
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "库管")]
        public ActionResult ViewStock()
        {
            return View();
        }


        [HttpGet]
        [Authorize(Roles = "库管")]
        public ActionResult GetStock(string datas,bool throughput=false)
        {

            try
            {
                var ids = datas.DeserializeFromJson<List<int>>(false);
                return Json(StoreBusinessObject.GetStock(ids,throughput), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "库管")]
        public ActionResult CheckStock(string datas)
        {
            try
            {
                
                var list = datas.DeserializeFromJson<List<StoreDetailModal>>(false);


                StoreBusinessObject.InsertOrder(User.Identity.Name, DonataMembership.GetUserNode(User.Identity.Name), "盘点", "",DateTime.Now, list);
                return Json(new iJsonResult("success"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }


        [Authorize]
        public ActionResult CashManager()
        {
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }
    }
}
