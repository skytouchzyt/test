using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using Tools;
namespace MvcDonata.Controllers
{
    

    public class QueryController : Controller
    {
        //
        // GET: /Query/

        

        public ActionResult Index()
        {

            //首先获取所有的controller
            var controllers = from t in this.GetType().Assembly.GetTypes()
                        where t.IsPublic && t.BaseType != null && t.BaseType.Name == "Controller"
                        select t;
            var query=from c in controllers
                      select new ControllerDescription
                      {
                           Name=c.Name.Substring(0,c.Name.Length-"Controller".Length),
                            Actions=(from a in c.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.DeclaredOnly|BindingFlags.InvokeMethod)
                                     select new ActionDescription
                                     {
                                           Name=a.Name,
                                            ArgNames=(from m in a.GetParameters()
                                                      select m.Name).ToList(),
                                            Method=(from ca in a.GetCustomAttributes(false)
                                                    where ca.ToString()=="System.Web.Mvc.HttpPostAttribute"
                                                    select ca).Count()==0?"get":"post"
                                     }).ToList()
                      };
            return Json(query, JsonRequestBehavior.AllowGet);
            
        }


        public ActionResult test()
        {
            dynamic http = new DynamicHttpRequest("127.0.0.1:34278");
            return Content(http.Customers.GetCustomer(-1));
        }

        public ActionResult Angular()
        {
            return View();
        }
    }
}
