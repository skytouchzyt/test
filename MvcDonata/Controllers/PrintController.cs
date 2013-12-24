using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Threading;
using System.Text;
using System.Windows;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Media;

namespace MvcDonata.Controllers
{
    public class PrintController : Controller
    {
        //
        // GET: /Print/

        public ActionResult Index()
        {
            Thread thread = new Thread(() =>
            {


                //var ps = new PrintServer(); //这里没有使用PrintServer,
                //如果是分单打印需要用到PrintServer选择不同的打印机 


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


                pd.PrintVisual(myPanel, "this is a test");//调用默认的打印机打印



            }
           );
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();


            thread.Join();//等待打印线程完成
            return View();
        }

    }
}
