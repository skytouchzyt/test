using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using help=System.Web.Helpers;
using System.Data.Objects;
using Tools;

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
             using (var m = new Models.DonataEntities())
            {
                ViewBag.list = (from d in m.DNS
                                where EntityFunctions.DiffHours(DateTime.Now,d.UpdatedTime)<24 //只显示24小时更新过的节点
                                orderby d.Node
                                select d
                             ).ToList();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(Models.DNS reg)
        {
            try
            {


                using (var m = new Models.DonataEntities())
                {

                    if (string.IsNullOrEmpty(reg.Node))
                        throw new Exception("请输入分店名");

                    var query = from d in m.DNS
                                where d.Node == reg.Node
                                select d;
                    if (query.Count() == 0)
                    {
                        m.DNS.AddObject(
                            new Models.DNS 
                            { 
                                Node = reg.Node, 
                                Port = reg.Port, 
                                IP = HttpContext.Request.UserHostAddress, 
                                UpdatedTime = DateTime.Now,
                                ApplicationVersion=reg.ApplicationVersion                           
                            });
                    }
                    else
                    {
                        var dns = query.First();
                        dns.IP = HttpContext.Request.UserHostAddress;
                        dns.Port = reg.Port;
                        dns.UpdatedTime = DateTime.Now;
                        dns.ApplicationVersion = reg.ApplicationVersion;

                    }
                    m.SaveChanges();


                }

            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;
            }
            finally
            {

            }
            return View(reg);
        }



        public ActionResult list()
        {
            ViewBag.list = Models.BusinessObject.GetNodes();
            return View();   
        }

        public ActionResult GetNodes(string server="")
        {
            if (string.IsNullOrEmpty(server))
            {
                return Json(Models.BusinessObject.GetNodes(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Content(("http://" + server + "/DNS/GetNodes").SendGetRequest());
            }
        }

        public string get(string node)
        {

            try
            {
                var m = new Models.DonataEntities();
                var query = from d in m.DNS
                            where d.Node == node
                            select d;
                if (query.Count() == 0)
                    throw new Exception("没有找到所需的分店名,请检查分店名是否正确,或者分店不在线.");

                var temp = query.First();

                return temp.IP;

                //return Json(
                //    new Models.iJsonResult(new { ip = temp.IP, port = temp.Port }),
                //    JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return "";
                //return Json(new Models.iJsonResult(null, e.Message),
                //JsonRequestBehavior.AllowGet
                //);
            }


        }

    }
}
