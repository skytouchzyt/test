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

        private static readonly int PageSize = 5;

        public ActionResult Index(string phone,string address,string page)
        {
            var p = 1;
            if (int.TryParse(page, out p))
            {
                if (p < 1) p = 1;
                Response.Cookies["Page"].Value =p.ToString();
                Response.Cookies["Page"].Expires = DateTime.MaxValue;
            }
            int pageCount = 0;
            
            Models.BusinessObject.GetCustomers(out pageCount, "", "", 1, PageSize);
            ViewBag.PageCount = pageCount;
            return View();
        }


        public ActionResult GetCustomerInfoByPhone(string phone,string server)
        {
            if (string.IsNullOrEmpty(server))
            {
                return Json(Models.BusinessObject.GetCustomer(phone), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Content(("http://" + server + "/Customers/GetCustomerInfoByPhone").SendGetRequest(new { phone = phone }, 10));
            }
        }

        [HttpPost]
        public ActionResult Page(string phone, string address, int page)
        {
            if (string.IsNullOrEmpty(phone))
                phone = "";
            if (string.IsNullOrEmpty(address))
                address = "";

            Response.Cookies["Page"].Value = page.ToString();
            Response.Cookies["Page"].Expires = DateTime.MaxValue;
            int pageCount = 0;
            var view=PartialView("Page", Models.BusinessObject.GetCustomers(out pageCount,phone, address, page));

            return view;
        }

        public ActionResult Edit(int ID)
        {
            ViewBag.Title = "编辑顾客资料";
            return View("Create",Models.BusinessObject.GetCustomer(ID));
        }
        [HttpPost]
        public ActionResult Edit(Models.CustomerModel cm)
        {
     
            //更新顾客资料

            Models.BusinessObject.UpdateCustomer(cm);

            return RedirectToAction("Index");
        }
        public ActionResult GetCustomerByID(int id)
        {
            if (id == 0)
            {
                return Json(new Models.CustomerModel(), JsonRequestBehavior.AllowGet);
            }
            return Json(Models.BusinessObject.GetCustomer(id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAddressesByID(int id)
        {
            if (id == 0)
            {
                //返回一个空Address对象
                //Thread.Sleep(3000);
                return Json(new Models.AddressModel(),JsonRequestBehavior.AllowGet);
            }
            return Json(Models.BusinessObject.GetAddressesByCustomerID(id),JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(string data)
        {
            try
            {
                var ser = new DataContractJsonSerializer(typeof(Models.CustomerModel));
                var customer = (Models.CustomerModel)ser.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(data)));

                if (customer.ID> 0)
                    Models.BusinessObject.UpdateCustomer(customer,true);
                else
                    Models.BusinessObject.InsertCustomer(customer);
                return View();
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View();
            }
        }


        /// <summary>
        /// 导入客户资料
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Import(string jsonData)
        {
            try
            {
                var customer = jsonData.DeserializeFromJson<Models.CustomerModel>();
                Models.BusinessObject.InsertCustomer(customer);
                return Json(new Models.iJsonResult("success"));
            }
            catch(Exception e)
            {
                return Json(new Models.iJsonResult(null, e.Message));
            }

        }

        [HttpPost]
        public ActionResult ImportPhoneOrder(string jsonData)
        {
            try
            {

                var list = jsonData.DeserializeFromJson<List<Models.PhoneOrderModel>>();
                foreach (var po in list)
                {
                    if (Models.BusinessObject.IsPhoneOrderExists(po))
                        return Json(new Models.iJsonResult("success"));

                    Models.BusinessObject.InsertPhoneOrder(po);

                }
                return Json(new Models.iJsonResult("success"));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, "导入外送单子出错:"+e.Message));
            }
        }
           
        


        public ActionResult ListAddresses(int customerID)
        {
            return PartialView(Models.BusinessObject.GetAddressesByCustomerID(customerID));
        }


        //public ActionResult GetCustomer(int start)
        //{
        //    //using (var entity = new Models.DonataEntities())
        //    //{
        //    //    var query = (from c in entity.Customers
        //    //                 orderby c.ID
        //    //                 select c).Skip(start).Take(1);
        //    //    var cu = query.FirstOrDefault();
        //    //    if (cu == null)
        //    //    {
        //    //        return Content("null");
        //    //    }

        //    //    var customer = new Customer
        //    //    {
        //    //        Addresses = (from a in Models.BusinessObject.GetAddressesByCustomerID(cu.ID)
        //    //                     select new Address
        //    //                     {
        //    //                         City = a.City,
        //    //                         Count = a.Count,
        //    //                         LastUsedTime = a.LastUsedTime,
        //    //                         Where = a.Address

        //    //                     }
        //    //                   ).ToList(),
        //    //        LastOrderTime = cu.LastOrderTime,
        //    //        Phones = cu.Phone.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
        //    //        Total = cu.Total
        //    //    };

                
        //    //    return Json(customer, JsonRequestBehavior.AllowGet);

        //    }
        //}
        //public ActionResult GetCustomersCount()
        //{
        //    using(var entity=new Models.DonataEntities())
        //    {
        //        return Content(entity.Customers.Count().ToString());
        //    }
        //}
    }
}
