using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
namespace MvcDonata.Controllers
{
    public class AdGearController : Controller
    {
        //
        // GET: /AdGear/

        public ActionResult Index()
        {
            return View();
        }
        static List<string> Accounts = new List<string>
        {
            "169790",
            "164022",
            "177072"
        };
        public string Verify(string account)
        {
            if (Accounts.Contains(account))
                return (account + "skytouchzyt".MD5()).MD5();
            else
                return "";
        }

    }
}
