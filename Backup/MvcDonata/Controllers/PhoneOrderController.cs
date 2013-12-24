using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
using MvcDonata.Models;

namespace MvcDonata.Controllers
{
    public class PhoneOrderController : Controller
    {
        //
        // GET: /PhoneOrder/
        [Authorize]
        public ActionResult Index(string phone)
        {
            return View();
            
        }

        [Authorize]
        public ActionResult NewPhoneOrder(string phone)
        {
            ViewBag.SoldoutDishes = Models.BusinessObject.GetSoldoutDishes().SerializeToJson(false);
            ViewBag.Phone = phone;
            return View();
        }

        
        public ActionResult GetPhoneOrders(
            bool paged,
            int pageSize,
            int pageIndex,
            string node,
            DateTime selectedDate,
            string server=""
            )
        {
            if (string.IsNullOrEmpty(server))
            {
                var tuple = Models.BusinessObject.GetPhoneOrders(paged, pageSize, pageIndex, node, selectedDate);
                return Json(

                    new
                    {
                        pageCount = tuple.Item1,
                        datas = tuple.Item2,
                        Amount = tuple.Item3,
                        nodes = Models.BusinessObject.GetNodesByOrders(selectedDate),
                        pageIndex = pageIndex
                    }

                    ,
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                var json = ("http://" + server + "/PhoneOrder/GetPhoneOrders").SendGetRequest
                    (
                        new
                        {
                            paged = paged,
                            pageSize = pageSize,
                            pageIndex = pageIndex,
                            node = node,
                            selectedDate = selectedDate
                        },
                        20
                    );
                return Content(json);

            }
        }

        public ActionResult GetPhoneOrderDetails(int orderID,string server="")
        {
            if (string.IsNullOrEmpty(server))
            {
                return Json(new { datas = Models.BusinessObject.GetPhoneOrderDetails(orderID) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Content(("http://" + server + "/PhoneOrder/GetPhoneOrderDetails").SendGetRequest(new { orderID = orderID }, 20));
            }
        }


        public string GetNewPhoneOrders(string node)
        {

            return Models.BusinessObject.GetNewPhoneOrders(node).SerializeToJson();
        }

        [HttpPost]
        public ActionResult SignPhoneOrder(string barcode)
        {
            return Json(Models.BusinessObject.SignPhoneOrder(barcode));
        }

        [HttpPost]
        public string CreateNewPhoneCall(int number,string phone, DateTime time)
        {
            try
            {
                Models.BusinessObject.NewPhoneCall(number, phone, DateTime.Now);
                return new iJsonResult("success").SerializeToJson();
            }
            catch (Exception e)
            {
                return new iJsonResult(null, e.Message).SerializeToJson();
            }
        }

        public string GetNewPhoneCall(int number)
        {
            try
            {
                return Models.BusinessObject.GetNewPhoneCall(number);
            }
            catch
            {
                return "";
            }

        }

        [HttpPost]
        public ActionResult UploadPhoneOrder(string jsonData,string server="")
        {
            try
            {
                if (string.IsNullOrEmpty(server)) //这是主服务器
                {
                    var po = jsonData.DeserializeFromJson<Models.PhoneOrderModel>(false);


                    Models.BusinessObject.InsertPhoneOrder(po);
                }
                else //这是本地服务器,需要上传到主服务器
                {
                    ("http://" + server + "/PhoneOrder/UploadPhoneOrder").SendPostRequest(
                        new { jsonData = jsonData }, 20);
                }

                
                return Json(new Models.iJsonResult("success"));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, e.Message));
            }
        }


        public ActionResult WatchPhone()
        {
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }
    }
}
