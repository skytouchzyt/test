using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
using Models.MongoDB;
using MvcDonata.Models;

namespace MvcDonata.Controllers
{
    public class DineOrderController : Controller
    {
        //
        // GET: /DineOrder/

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListDineOrder(string node, DateTime selectedDate, int pageIndex)
        {
            return View();
        }

        public ActionResult UploadDineOrder()
        {
            dynamic config = HttpContext.Application["config"];
            string mode = config.服务器模式;
            string server = config.服务器地址;
            if(mode!="主服务器")
            {
                var needed = Models.BusinessObject.GetNeededUploadDineOrder();
                var r = ("http://"+server + "/dineorder/InsertDineorder")
                            .SendPostRequest(new { data = needed.SerializeToJson(false) })
                            .DeserializeFromJson<Models.iJsonResult>();
                if (r.successed)
                {
                    Models.BusinessObject.SignDineOrders(needed);
                }

            }
            return Content("");
        }

        [HttpPost]
        public ActionResult InsertDineOrder(string data)
        {
            try
            {

                Models.BusinessObject.InsertDineOrder(data.DeserializeFromJson<List<DineOrder>>(false));
                var r = new Models.iJsonResult
                {
                    successed = true
                };
                return Json(r);
            }
            catch (Exception e)
            {
                var r = new Models.iJsonResult
                {
                    successed = false,
                     errorMessage=e.Message
                };
                return Json(r);
            }
            
        }

        public ActionResult CreateNewDineOrderBarCode(string node)
        {
            try
            {
                var r = new Models.iJsonResult
                {
                    successed = true,
                    result = Models.BusinessObject.CreateNewDineOrderBarCode(node)
                };
                return Json(r, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                var r = new Models.iJsonResult
                {
                    successed = false,
                    errorMessage=e.Message
                };
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetDineOrders(
            bool paged,
            int pageSize,
            int pageIndex,
            string node,
            DateTime selectedDate
            )
        {

            return Json(Models.BusinessObject.GetDineOrders(paged, pageSize, pageIndex, node,selectedDate),           
                JsonRequestBehavior.AllowGet);
        }


        public string CalcTotalDineOrders()
        {
            dynamic config = HttpContext.Application["config"];
            string mode = config.服务器模式;
            string server = config.服务器地址;
            string node = config.分店;
            return BusinessObject.CalcTotalDineOrders(node).ToString();
        }


        public ActionResult BarPad()
        {
            return View();
        }

        class BarInfo
        {
            public int Index { get; set; }
            public string Bar { get; set; }
            public DateTime LastUpdatedTime { get; set; }
        }
        public ActionResult GetDishStatus(string bar, int mins,int index)
        {

            var app = HttpContext.Application;
            dynamic config = app["config"];
            string database = config.数据库;
            try
            {
                app.Lock();
                var list=new List<BarInfo>();
                if (app["Bars"] != null)
                    list = (List<BarInfo>)app["Bars"];
                
                //首先删除过期的设备
                list = (from b in list
                        where (DateTime.Now - b.LastUpdatedTime).TotalSeconds < 20
                        select b).ToList();


                var query = from b in list
                            where b.Bar == bar && b.Index == index
                            select b;
                if (query.Count() == 0) //没有此设备就加入
                {
                    list.Add(new BarInfo
                    {
                        Bar = bar,
                        Index = index,
                        LastUpdatedTime=DateTime.Now
                    });
                }
                else
                {
                    query.First().LastUpdatedTime = DateTime.Now;
                }
                var max = (from b in list
                           where b.Bar == bar
                           select b.Index).Max();
                app["Bars"] = list;
                app.UnLock();


                return Json(Models.PadBusiness.GetDishStatusByBar_mongodb(bar, new TimeSpan(0, mins, 0), index, max), JsonRequestBehavior.AllowGet);


            }
            catch(Exception e)
            {
                app.UnLock();
                return Content("");
            }
        }

        [HttpPost]
        public ActionResult UploadDishStatus(string datas)
        {
            var app = HttpContext.Application;
            dynamic config = app["config"];
            string database = config.数据库;
            try
            {

                    var list = datas.DeserializeFromJson<List<DishStatus>>(false);

                    Models.PadBusiness.InsertDishStatus(list);
                
                return Json(new Models.iJsonResult("success", null));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, e.Message));
            }

        }

        [HttpPost]
        public ActionResult UpdateDishStatus(string statusID, string status)
        {
            try
            {
                var app = HttpContext.Application;
                dynamic config = app["config"];
                string database = config.数据库;


                    Models.PadBusiness.UpdateDishStatus(statusID, status);
                
                return Json(new Models.iJsonResult("success", null));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, e.Message));
            }
        }

        [HttpPost]
        public ActionResult ChangeTable(string barCode, string newTable)
        {
            var app = HttpContext.Application;
            dynamic config = app["config"];
            string database = config.数据库;


                Models.PadBusiness.ChangeTable(barCode, newTable);
            
            return Json(new Models.iJsonResult("success"));
        }
    }
}
