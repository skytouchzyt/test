using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Tools;
namespace MvcDonata.Controllers
{
    public class FileController : Controller
    {
        //
        // GET: /File/

        public FileStreamResult DownloadFile(string file)
        {
            return File(System.IO.File.Open(file,FileMode.Open), "application", file);
        }

        [HttpPost]
        public ActionResult UploadFile(string rootDirectory,string sourceFile,string verifyCode)
        {
            var local=DateTime.Now.TimeStamp();
            try
            {
                if (local != verifyCode)
                {
                    throw new Exception("检测校验码失败!");
                }
                foreach (string upload in Request.Files)
                {
                    if (!HasFile(Request.Files[upload])) continue;
                    string filename = sourceFile.ReplaceRootDirectory(rootDirectory, serverRoot); //映射到服务器目录下
                    var parent=Directory.GetParent(filename);
                    if (!parent.Exists)
                    {
                        parent.Create();
                    }
                    Request.Files[upload].SaveAs(filename);
                }
                return Json(new Models.iJsonResult("success"));
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, e.Message));
            }
        }
        string serverRoot = AppDomain.CurrentDomain.BaseDirectory;


        
        public ActionResult ListFiles(string verifyCode)
        {
            try
            {
                if (DateTime.Now.TimeStamp() != verifyCode)
                {
                    throw new Exception("检测校验码失败!");
                }


                return Json(serverRoot.ScanDirectory().OrderBy(
                    fi=>
                        {
                            //让mvcdonata.dll排到最后,这样此文件最后才更新
                            if (fi.File.IndexOf("mvcdonata.dll", StringComparison.CurrentCultureIgnoreCase) >= 0)
                                return "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ";
                            else
                                return fi.File;
                        }
                        ), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new Models.iJsonResult(null, e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        private static bool HasFile(HttpPostedFileBase file)
        {
            return (file != null && file.ContentLength > 0) ? true : false;
        }


    }
}
