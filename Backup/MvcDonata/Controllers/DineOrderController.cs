using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
namespace MvcDonata.Controllers
{
    public class DineOrderController : Controller
    {
        //
        // GET: /DineOrder/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListDineOrder(string node, DateTime selectedDate, int pageIndex)
        {
            return View();
        }

        [HttpPost]
        public string UploadDineOrder(string data, string verifyCode)
        {
            try
            {
                var md5 = data.MD5();
                if (!string.IsNullOrEmpty(verifyCode)&&data.MD5() != verifyCode) //如果校验码不为空就检验数据是否有误
                {
                    throw new Exception("数据校验错误");
                }
                Models.BusinessObject.InsertDineOrder(data.DeserializeFromJson<Models.DineOrderModel>());
                return new Models.iJsonResult("success", null).SerializeToJson();
            }
            catch (Exception e)
            {
                return new Models.iJsonResult(null, e.Message).SerializeToJson();
            }
            
        }

        public ActionResult CreateNewBarCode(string node,string server)
        {
            try
            {
                return Json(new Models.iJsonResult("success", Models.BusinessObject.CreateNewBarCode(node, server)), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult("fail", e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLastBarCode(string node)
        {
            return Content(Models.BusinessObject.GetLastDineOrderBarCode(node));
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

        public ActionResult GetDineOrderDetails(int OrderID)
        {
            return Json(Models.BusinessObject.GetDineOrderDetails(OrderID), JsonRequestBehavior.AllowGet);
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

                return Json(Models.PadBusiness.GetDishStatusByBar(bar, new TimeSpan(0, mins, 0),index,max), JsonRequestBehavior.AllowGet);
            }
            catch
            {
                app.UnLock();
                return Content("");
            }
        }

        [HttpPost]
        public ActionResult UploadDishStatus(string datas)
        {

            try
            {
                var list = datas.DeserializeFromJson<List<Models.DishStatusModel>>(false);
               
                Models.PadBusiness.InsertDishStatus(list);
                return Json(new Models.iJsonResult("success", null));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, e.Message));
            }

        }

        [HttpPost]
        public ActionResult UpdateDishStatus(int statusID, string status)
        {
            try
            {
                string.Format("接收到餐品状态:{0}",statusID).Log(@"d:\donata\www.txt");
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
            Models.PadBusiness.ChangeTable(barCode, newTable);
            return Json(new Models.iJsonResult("success"));
        }
    }
}
