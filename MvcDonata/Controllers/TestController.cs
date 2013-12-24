using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
using System.Diagnostics;


using System.Threading;
using System.Text;
using System.Windows;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Media;

namespace MvcDonata.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Audio()
        {
            return View();
        }

        public ActionResult Command()
        {
            return View();
        }
        public ActionResult Execute(string command)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;



            p.StartInfo.RedirectStandardOutput = true;



            p.StartInfo.RedirectStandardError = true;


            p.StartInfo.CreateNoWindow = true;

            p.Start();

            p.StandardInput.WriteLine(command);
            p.StandardInput.WriteLine("exit");


            return Content(p.StandardOutput.ReadToEnd().Replace("\n", "<br/>"));


        }



        public ActionResult Canvas()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult CalcStep(string json)
        //{
        //    var steps = json.DeserializeFromJson<List<Models.Five.Step>>(false);
        //    var time1 = DateTime.Now;
        //    var r = Models.Five.Computer.Sim(steps, 3);
        //    var time2 = DateTime.Now;

        //    return Json(r);
        //}

        //[HttpPost]
        //public ActionResult CalcInfo(string json, int row, int col)
        //{
        //    var steps = json.DeserializeFromJson<List<Models.Five.Step>>(false);
        //    var table = new Models.Five.Table(steps);
        //    table.CalcAllSituations();
        //    return Json(table.GetPositionInfo(row, col));
        //}


        public ActionResult BackBone()
        {
            return View();
        }

        public ActionResult Ember()
        {
            return View();
        }

        public ActionResult BaiduMap()
        {
            return View();
        }

        public ActionResult xml()
        {
            
            return View();
        }

        /// <summary>
        /// 模拟WP8动态磁贴效果
        /// </summary>
        /// <returns></returns>
        public ActionResult Motion()
        {
            return View();
        }

        public ActionResult print(string printer)
        {
            //建立一个STA线程,然后调用PrintDialog打印(WPF中UI控件只能使用STA线程才能调用)
            Thread thread = new Thread(() =>
                {


                    //var ps = new PrintServer(); //这里没有使用PrintServer,如果是分单打印需要用到PrintServer选择不同的打印机 


                    var pd = new PrintDialog();
                    StackPanel myPanel = new StackPanel();


                    myPanel.Margin = new Thickness(0.2);


                    myPanel.Children.Add(new TextBlock
                    {
                        Text = "欢迎光临",
                        FontSize = 24.0f,
                        TextAlignment = TextAlignment.Center,
                        FontFamily = new FontFamily("Microsoft YaHei"),


                    });



                    myPanel.Measure(new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight));
                    myPanel.Arrange(new Rect(new Point(0, 0), myPanel.DesiredSize));






                    //首先收到打印机
                    var ps = new PrintServer();

                    var queues = ps.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });

                    PrintQueue pq = null;

                    foreach (var q in queues)
                    {
                        if (q.Name == printer)
                        {
                            pq = q;
                            break;
                        }
                    }
                    if (pq == null)
                        throw new Exception(string.Format("没有找到名为[{0}]的打印机.", printer));

                    pd.PrintQueue = pq;
                    pd.PrintVisual(myPanel, "this is a test");//调用默认的打印机打印
                }


            );

            thread.SetApartmentState(ApartmentState.STA); 
            thread.Start();


            thread.Join();//等待打印线程完成


            return Content("");
        }


        public ActionResult Augular()
        {
            return View();
        }
    }
}
