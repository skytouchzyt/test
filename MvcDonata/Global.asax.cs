using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Tools;

namespace MvcDonata
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {

        private static object obj;
        private static object locker=new object();
        public object Obj
        {
            get
            {
                lock (locker)
                {
                    return obj;
                }
            }
            set
            {
                lock (locker)
                {
                    obj = value;
                }
            }
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("NoAction", "{controller}", new { controller = "home", action = "index", id = "" });//无Action的匹配
            routes.MapRoute("NoID", "{controller}/{action}", new { controller = "home", action = "index", id = "" });//无ID的匹配

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute("Root", "", new { controller = "home", action = "index", id = "" });//根目录匹配
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            try
            {
                var path = @"e:\donata";
                path = path + "\\donata.xml";

                path.Log("e:\\wwwlog.txt");

                dynamic config = path.LoadXElement();
                HttpContext.Current.Application["config"] = config;

                Models.BusinessObject.mongodbAddress = config.数据库地址;
            }
            catch (Exception e)
            {
                e.Message.Log("e:\\wwwlog.txt");
            }


            
        }

        
    }
}