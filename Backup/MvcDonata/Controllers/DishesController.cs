using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
namespace MvcDonata.Controllers
{
    public class DishesController : Controller
    {
        //
        // GET: /Dishes/

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string node)
        {
            return Json(Models.BusinessObject.GetDishes(node,Models.PhoneOrderType.外卖));
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(string datas)
        {
            try
            {
                var dish=datas.DeserializeFromJson<Models.DishModel>(false);
                Models.BusinessObject.InsertDish(dish);
                return Json(new Models.iJsonResult("success"));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult("fail", e.Message));
            }
            
        }

        [HttpPost]
        public ActionResult UploadDishes(string datas)
        {
            try
            {
                var dishes = datas.DeserializeFromJson<List<Models.DishModel>>(false);
                Models.BusinessObject.UpdateAllDishes(dishes);
                return Json(new Models.iJsonResult("success"));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult("fail", e.Message));
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(string datas)
        {
            try
            {
                var dish = datas.DeserializeFromJson<Models.DishModel>(false);
                Models.BusinessObject.UpdateDish(dish);
                return Json(new Models.iJsonResult("success"));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult("fail", e.Message));
            }
        }


        [Authorize]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                Models.BusinessObject.DeleteDish(id);
                return Json(new Models.iJsonResult("success", null));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult("fail", e.Message));
            }
            
        }

        [Authorize]
        public ActionResult GetDishesForEdit()
        {
            return Json(Models.DishesBusinessObject.GetDishes(), JsonRequestBehavior.AllowGet);
        }


        public string GetDishes(string node,string orderType,bool encoding=true)
        {
            return Models.BusinessObject.GetDishes(node,
                (Models.PhoneOrderType)Enum.Parse(typeof(Models.PhoneOrderType), orderType)
                ).SerializeToJson(encoding);
        }

        public string SetSoldOutDishes(string node, string dishName, bool soldOut)
        {
            try
            {
                Models.BusinessObject.SetSoldOutDish(node, dishName, soldOut);
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public ActionResult GetSoldoutDishes()
        {
            return Json(Models.BusinessObject.GetSoldoutDishes(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBars()
        {
            try
            {

                return Json(Models.BusinessObject.GetBars(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, e.Message));
            }
        }

    }
}
