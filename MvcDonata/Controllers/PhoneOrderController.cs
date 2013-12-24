using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
using MvcDonata.Models;
using Models.MongoDB;

namespace MvcDonata.Controllers
{
    public class PhoneOrderController : Controller
    {
        //
        // GET: /PhoneOrder/
        [Authorize]
        public ActionResult Index(string phone)
        {
            dynamic config = HttpContext.Application["config"];
            string mode = config.服务器模式;
            string server = config.服务器地址;
            string node = config.分店;
            ViewBag.mode = mode;
            return View();
            
        }

        /// <summary>
        /// 呼叫中心点餐网页
        /// </summary>
        /// <param name="phone">传入电话,网页根据电话查询顾客,取得顾客资料</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult NewPhoneOrder(string phone)
        {
            ViewBag.Phone = phone;
            return View();
        }


        public string CalcTotalPhoneOrders()
        {
            dynamic config = HttpContext.Application["config"];
            string mode = config.服务器模式;
            string server = config.服务器地址;
            string node = config.分店;
            return BusinessObject.CalcTotalPhoneOrders(node).ToString();
        }

        



        public ActionResult GetPhoneOrders(
            bool paged,
            int pageSize,
            int pageIndex,
            string node,
            DateTime selectedDate
            )
        {
            dynamic config = HttpContext.Application["config"];
            string mode = config.服务器模式;
            string server = config.服务器地址;
            try
            {
                if (mode == "主服务器")
                {

                    //string.Format("paged={0},pageSize={1},pageIndex={2},node={3},selectedDate={4}", paged, pageSize, pageIndex, node, selectedDate).Log(@"e:\log\getPhoneOrders.txt");

                    //如果没有登录，就说明是呼叫中心服务器中转过来的请求
                    bool isAdmin = false;
                    if (string.IsNullOrEmpty(User.Identity.Name))
                    {
                        isAdmin = false;
                    }
                    else
                    {
                        isAdmin = User.IsInRole("网管");
                    }



                    var tuple = Models.BusinessObject.GetPhoneOrders(paged, pageSize, pageIndex, node, selectedDate, isAdmin);
                    return Json(

                        new
                        {
                            pageCount = tuple.Item1,
                            datas = tuple.Item2,
                            Amount = tuple.Item3,
                            nodes = Models.BusinessObject.GetNodes(),
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
            catch(Exception e)
            {
                return Json(
                    new iJsonResult
                    {
                        successed = false,
                        errorMessage = e.Message
                    }, JsonRequestBehavior.AllowGet);
            }
        }




        public string GetNewPhoneOrders(string node)
        {
            dynamic config = HttpContext.Application["config"];
            string mode = config.服务器模式;
            string server = config.服务器地址;
            string database = config.数据库;
            if (mode == "主服务器")
            {

                    return Models.BusinessObject.GetNewPhoneOrders(node).SerializeToJson();

            }
            else
            {
                //先从主服务器接收新的外送订单
                //然后写入本地数据库
                //如果本地数据库已经有此订单就说明此订单先前被接收到过,不再发往客户端
                //如果订单已经超过30分钟也不发往客户端
                //如果订单已经打印过,也不要发往客户端
                var list = ("http://" + server + "/phoneorder/getnewphoneorders")
                        .SendGetRequest(new { node = node }, 30).DeserializeFromJson<List<PhoneOrder>>();
                var temp = new List<PhoneOrder>();
                foreach (var po in list)
                {
                    if (Models.BusinessObject.InsertPhoneOrder(po)&&
                        (DateTime.Now-DateTime.Parse(po.CreatedTime)).TotalMinutes<30)
                    {
                        temp.Add(po);
                    }
                }
                return temp.SerializeToJson();
            }

        }


        [HttpPost]
        public ActionResult SignPhoneOrder(string data=null)
        {
            dynamic config = HttpContext.Application["config"];
            string mode = config.服务器模式;
            string server = config.服务器地址;
            string node=config.分店;
            try
            {
                if (mode == "主服务器")
                {
                    if(!string.IsNullOrEmpty(data)) //为空就跳过
                        Models.BusinessObject.SignPhoneOrder(data.DeserializeFromJson<List<string>>(false));
                }
                else
                {
                    if (string.IsNullOrEmpty(data)) //如果为空,就是让本地服务器告诉主服务器外送单子接收到了
                    {
                        var signs = Models.BusinessObject.GetNewPhoneOrders(node);
                        var bars = signs.Where(i=>i.Printed).Select(i => i.BarCode).ToList(); //被打印过的单子才告诉呼叫中心已被接收
                        if (signs.Count() > 0)
                        {
                            var r = ("http://" + server + "/phoneorder/signphoneorder").SendPostRequest(
                            new
                            {
                                data = bars.SerializeToJson(false)
                            }, 30).DeserializeFromJson<iJsonResult>(false);
                            if (!r.successed)
                                return Json(r);
                            Models.BusinessObject.SignPhoneOrder(bars);
                        }
                    }
                    else //如果不为空,就是客户端告诉本地服务器外送订单已经打印过
                    {
                        Models.BusinessObject.SignPhoneOrder(data.DeserializeFromJson<List<string>>(false),true);
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new iJsonResult
                {
                    successed = false,
                    errorMessage = e.Message
                }
                );
            }
            return Json(new iJsonResult
            {
                successed = true
            }
                );
        }

        [HttpPost]
        public string CreateNewPhoneCall(string number,string phone, DateTime time)
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

        public string GetNewPhoneCall(string number)
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

        /// <summary>
        /// 呼叫中心通过此函数提交外卖订单
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadPhoneOrder(string jsonData)
        {
            try
            {
                dynamic config = HttpContext.Application["config"];
                string mode = config.服务器模式;
                string server = config.服务器地址;
                if (mode=="主服务器") //这是主服务器
                {
                    
                    
                    var po = jsonData.DeserializeFromJson<PhoneOrder>(false);


                    Models.BusinessObject.UploadPhoneOrder(po);
                }
                else //这是本地服务器,需要上传到主服务器
                {
                    jsonData.Log(@"e:\log\uploadphoneorder.txt");

                    //上传到主服务器
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
