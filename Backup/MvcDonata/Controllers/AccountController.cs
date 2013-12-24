using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using MvcDonata.Models;
using Tools;
using help = System.Web.Helpers;
namespace MvcDonata.Controllers
{
    public class AccountController : Controller
    {

        //
        // GET: /Account/LogOn

        public ActionResult LogOn()
        {
            return View();
        }

        //
        // POST: /Account/LogOn

        public bool ValidateUser(string user, string password)
        {
            return DonataMembership.ValidateUser(user, password) != null;

        }


        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                bool valid;
                if (string.IsNullOrEmpty(model.Server))
                {
                    var e = DonataMembership.ValidateUser(model.UserName, model.Password);
                    valid = e != null;
                }
                else
                {

                    valid = bool.Parse(("http://" + model.Server + "/Account/ValidateUser")
                        .SendGetRequest(
                            new
                            {
                                user = model.UserName,
                                password = model.Password
                            }));

                }
                if (valid)
                {

                    var ticket = new FormsAuthenticationTicket(
                        1,
                        model.UserName,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(15),
                        true,
                        new DonataRoleProvider().GetRolesForUser(model.UserName).GetString(),
                        "/");

                    var hashTicket = FormsAuthentication.Encrypt(ticket);

                    Response.Cookies.Add(
                            new HttpCookie(
                                FormsAuthentication.FormsCookieName,
                                hashTicket
                                )
                                );

                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);




                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "用户名或者密码输入错误.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register
        [Authorize(Roles = "网管")]
        public ActionResult Register()
        {
            ViewBag.Title = "注册";
            ViewBag.h2 = "创建员工";
            return View(new EmployeeModel());
        }

        //
        // POST: /Account/Register
        [Authorize(Roles = "网管")]
        [HttpPost]
        public ActionResult Register(EmployeeModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    DonataMembership.CreateUser(model);
                    //FormsAuthentication.SetAuthCookie(model.Name, false /* createPersistentCookie */);
                    ViewBag.Message = string.Format("创建员工{0}成功.", model.Name);
                    return View(new EmployeeModel());
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }
            }
            else
            {

            }
            ViewBag.ModelState = ModelState;
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    DonataMembership.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    return RedirectToAction("ChangePasswordSuccess");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }

            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess
        [Authorize]
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        [Authorize(Roles = "网管")]
        public ActionResult EditEmployee(int id)
        {


            ViewBag.Title = "修改";
            ViewBag.h2 = "更新员工信息";



            using (var entity = new Models.DonataEntities())
            {
                var query = from e in entity.Employee
                            where e.ID == id
                            select e;
                var r=from e in query.ToList()
                      select e.Convert<Models.EmployeeModel>(null);
               
                if (r.Count() > 0)
                {
                    return View(r.First());
                }
                else
                {
                    return RedirectToAction("ListEmployees");
                }
            }

        }
        [Authorize(Roles = "网管")]
        [HttpPost]
        public ActionResult EditEmployee(EmployeeModel model)
        {
            try
            {


                using (var entity = new Models.DonataEntities())
                {
                    var query = from e in entity.Employee
                                where e.ID == model.ID
                                select e;
                    if (query.Count() == 0)
                        throw new Exception("没有找到指定的员工");
                    var em = query.First();

                    model.Convert<Models.Employee>(em);

                    entity.SaveChanges();

                    return RedirectToAction("ListEmployees");
                }
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;
            }
            return View(model);
        }

        //获取员工列表
        [Authorize(Roles = "网管")]
        public ActionResult ListEmployees()
        {

            using (var entity = new Models.DonataEntities())
            {
                var query = from e in entity.Employee
                            orderby e.Node, e.EntryDate
                            group e by e.Node into g
                            select g;



                return View(query.ToList());

            }

        }

        //获取员工
        [Authorize(Roles = "网管")]
        public JsonResult get(int id)
        {
            try
            {
                using (var entity = new Models.DonataEntities())
                {
                    var query = from e in entity.Employee
                                where e.ID == id
                                select e;
                    var r = from e in query.ToList()
                            select new
                            {
                                ID = e.ID,
                                Actived = e.Actived,
                                BankCard = e.BankCard,
                                EntryDate = e.EntryDate.ToString("yyyy-MM-dd"),
                                Name = e.Name,
                                Node = e.Node,
                                Number = e.Number,
                                Phone = e.Phone,
                                Post = e.Post,
                                Salary = e.Salary
                            };
                    if (query.Count() == 0)
                        throw new Exception("没有找到员工");
                    else
                        return Json(new iJsonResult(r.First()), JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception e)
            {
                return Json(new iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }


    }
}
