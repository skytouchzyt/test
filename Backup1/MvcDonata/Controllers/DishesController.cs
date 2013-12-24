using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
using Models.MongoDB;
using MvcDonata.Models;


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
            return Json(Models.BusinessObject.GetDishes());
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(string datas)
        {
            try
            {
                var dish=datas.DeserializeFromJson<Dish>(false);
                Models.BusinessObject.UpdateInsertDishes(new List<Dish>{dish});
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

                var dishes = datas.DeserializeFromJson<List<Dish>>(false);
                Models.BusinessObject.UpdateInsertDishes(dishes);
                return Json(new Models.iJsonResult("success"));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult("fail", e.Message));
            }
        }


        [HttpPost]
        public ActionResult Edit(string datas)
        {
            try
            {
                var dish = datas.DeserializeFromJson<Dish>(false);
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

        /// <summary>
        /// 呼叫中心点餐时候调用
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDishesForPhoneOrder()
        {
            try
            {
                var dishes = BusinessObject.GetDishesWithCount(false);
                var r = new iJsonResult
                {
                    successed = true,
                    result = dishes.SerializeToJson(false)
                };
                return Json(r, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, false, e.Message));
            }
        }

        /// <summary>
        /// 堂食客户端加载时调用,或者编辑餐品时调用
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public ActionResult GetDishes(bool all=false)
        {
            dynamic config = HttpContext.Application["config"];
            string mode = config.服务器模式;
            string server = config.服务器地址;
            string database = config.数据库;
            if (mode == "主服务器")
            {
                try
                {
                    var dishes = BusinessObject.GetDishes(all);
                    var r = new iJsonResult
                    {
                        successed = true,
                        result = dishes.SerializeToJson(false)
                    };
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new Models.iJsonResult(null, false, e.Message));
                }
            }
            else
            {
                var result = ("http://" + server + "/Dishes/GetDishes")
                        .SendGetRequest(new { all = all }, 30).DeserializeFromJson<iJsonResult>();
                var list = result.result.ToString().DeserializeFromJson<List<Dish>>(false);
                Models.BusinessObject.UpdateInsertDishes(list);
                try
                {
                    var dishes = BusinessObject.GetDishesWithCount();
                    var r = new iJsonResult
                    {
                        successed = true,
                        result = dishes.SerializeToJson()
                    };
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new Models.iJsonResult(null, false, e.Message));
                }
            }
        }



        public ActionResult SetSoldOutDishes(string node, string dishName, bool soldOut)
        {
            try
            {
                Models.BusinessObject.SetSoldOutDish(node, dishName, soldOut);
                return Json(new iJsonResult { successed = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new iJsonResult { successed = false, errorMessage=e.Message }, JsonRequestBehavior.AllowGet);
            }
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
