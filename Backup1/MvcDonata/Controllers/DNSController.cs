using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using help=System.Web.Helpers;
using System.Data.Objects;
using Tools;
using Models.MongoDB;
using MvcDonata.Models;

namespace MvcDonata.Controllers
{
    public class DNSController : Controller
    {

        /// <summary>
        /// DNS首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Index(DNS dns)
        {
            try
            {
                dynamic config = HttpContext.Application["config"];
                string mode = config.服务器模式;
                string server = config.服务器地址;
                string node = config.分店;
                if (mode == "主服务器")
                {
                    dns.LastUpdatedTime = DateTime.Now.ToString(BusinessObject.datetimeFormatString);
                    dns.IP = HttpContext.Request.UserHostAddress;
                    Models.BusinessObject.UpsertDNS(dns);
                }
                else
                {
                    dns.LocalServerVersion = BusinessObject.Version;
                    dns.Node = node;
                    dns.Port = 8080;
                    return Content(("http://" + server + "/dns").SendPostRequest(dns));
                }


                return Json(new iJsonResult
                {
                     successed=true
                }
                );
            }
            catch (Exception e)
            {
                return Json(new iJsonResult
                {
                    successed = false,
                    errorMessage=e.Message
                }
                );
            }
        }



        public ActionResult GetDNSlist()
        {
            try
            {
                dynamic config = HttpContext.Application["config"];
                string mode = config.服务器模式;
                string server = config.服务器地址;

                if (mode == "主服务器")
                {

                    return Json(new iJsonResult
                    {
                        successed = true,
                        result = BusinessObject.GetDNSes().SerializeToJson()
                    },
                    JsonRequestBehavior.AllowGet
                    );
                }
                else
                {
                    var r = ("http://" + server + "/DNS/GetDNSList").SendGetRequest();
                    return Content(r);
                }
            }
            catch (Exception e)
            {
                return Json(new iJsonResult
                {
                    successed = false,
                    errorMessage = e.Message
                },
                JsonRequestBehavior.AllowGet
                );
            }
        }





    }
}
