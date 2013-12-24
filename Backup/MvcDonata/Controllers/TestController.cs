//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using Tools;
//using System.Diagnostics;



//namespace MvcDonata.Controllers
//{
//    public class TestController : Controller
//    {
//        //
//        // GET: /Test/

//        public ActionResult Index()
//        {
//            return View();
//        }

//        public ActionResult Audio()
//        {
//            return View();
//        }

//        public ActionResult Command()
//        {
//            return View();
//        }
//        public ActionResult Execute(string command)
//        {
//            Process p = new Process();
//            p.StartInfo.FileName = "cmd.exe";
//            p.StartInfo.UseShellExecute = false;
//            p.StartInfo.RedirectStandardInput = true;



//            p.StartInfo.RedirectStandardOutput = true;



//            p.StartInfo.RedirectStandardError = true;


//            p.StartInfo.CreateNoWindow = true;

//            p.Start();

//            p.StandardInput.WriteLine(command);
//            p.StandardInput.WriteLine("exit");


//            return Content(p.StandardOutput.ReadToEnd().Replace("\n","<br/>"));


//        }



//        public ActionResult Canvas()
//        {
//            return View();
//        }

//        [HttpPost]
//        public ActionResult CalcStep(string json)
//        {
//            var steps = json.DeserializeFromJson<List<Models.Five.Step>>(false);
//            var time1=DateTime.Now;
//            var r=Models.Five.Computer.Sim(steps, 3);
//            var time2 = DateTime.Now;

//            return Json(r);
//        }

//        [HttpPost]
//        public ActionResult CalcInfo(string json, int row,int col)
//        {
//            var steps = json.DeserializeFromJson<List<Models.Five.Step>>(false);
//            var table = new Models.Five.Table(steps);
//            table.CalcAllSituations();
//            return Json(table.GetPositionInfo(row, col));
//        }
//    }
//}
