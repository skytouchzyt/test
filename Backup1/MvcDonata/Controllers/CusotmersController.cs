using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
using System.Threading;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

using Models.MongoDB;

namespace MvcDonata.Controllers
{

    

    public class CustomersController : Controller
    {
        //
        // GET: /Cusotmers/



        public ActionResult Index(string phone,string address,string page)
        {
           
            return View();
        }


        public ActionResult GetCustomerInfoByPhone(string phone)
        {
            dynamic config=HttpContext.Application["config"];
            string server = config.服务器地址;
            string mode = config.服务器模式;
            if (mode=="主服务器")
            {
                return Json(Models.BusinessObject.GetCustomerByPhone(phone), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Content(("http://" + server + "/Customers/GetCustomerInfoByPhone").SendGetRequest(new { phone = phone }, 10));
            }
        }

        [HttpPost]
        public ActionResult SaveCustomer(string datas,string newPhone)
        {
            var customer = datas.DeserializeFromJson<Customer>(false);

            dynamic config = HttpContext.Application["config"];
            string server = config.服务器地址;
            string mode = config.服务器模式;

            try
            {
                if (mode == "主服务器")
                {
                    Models.BusinessObject.SaveCustomer(customer,newPhone);
                    return Json(new Models.iJsonResult(null, true));
                }
                else
                {
                    var r = ("http://" + server + "/Customers/SaveCustomer").SendPostRequest(new { datas = datas,newPhone=newPhone }, 20).DeserializeFromJson<Models.iJsonResult>(false);
                    return Json(r);
                }
                
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, false, e.Message));
            }
        }








        public ActionResult Create()
        {
            return View();
        }






           
        



        [Authorize]
        public ActionResult RegisterVIP()
        {
            var url = HttpContext.Request.Url;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult SendCode(string phone)
        {
            try
            {
                var r = new Random();
                var code = r.Next(1000000);
                using (var client = new WebService.LinkWSSoapClient())
                {
                    var sms = string.Format("注册验证码为：{0:d6}。", code);
                    client.Send("tclkj02192", "zytzly97", phone, sms, "", "");
                }
                return Json(new Models.iJsonResult(string.Format("{0:d6}", code)));
            }
            catch(Exception e)
            {
                return Json(new Models.iJsonResult(null, e.Message));
            }
                        
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddNewVIP(string phone, string number)
        {
            
            try
            {
                Models.BusinessObject.InsertNewVIP(phone, number, DonataMembership.GetUserNode(User.Identity.Name));
                return Json(new Models.iJsonResult("success"));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, e.Message));
            }
        }

        /// <summary>
        /// 查询VIP是否存在
        /// </summary>
        /// <param name="number">VIP号码</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult IsVIPExists(string number)
        {
            dynamic config = HttpContext.Application["config"];
            string mode = config.服务器模式;
            string server = config.服务器地址;
            try
            {
                if (mode == "主服务器")
                {


                    return Json(new Models.iJsonResult
                    {
                        result = Models.BusinessObject.GetCustomerByNumber(number) != null,
                        successed = true
                    }
                    );
                }
                else
                {
                    return Content(("http://" + server + "/Customers/IsVipExists").SendPostRequest
                        (new { number = number }, 30));
                }
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult
                {
                    successed = false,
                    errorMessage=e.Message
                });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetVIPByPhone(string phone)
        {
            try
            {
                var c = Models.BusinessObject.GetCustomerByPhone(phone);
                 return Json(new Models.iJsonResult(c,true));

            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null,false, e.Message));
            }
        }

        [HttpPost]
        public ActionResult Charge(string phone, string number, int value)
        {
            try
            {
                var left = Models.BusinessObject.Charge(phone, number, value);
                return Json(new Models.iJsonResult(left, true));
            }
            catch(Exception e)
            {
                return Json(new Models.iJsonResult(null, false, e.Message));
            }
        }

   
    }
}
