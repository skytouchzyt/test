using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Tools;
using System.Xml.Linq;
using System.Threading;
using System.Text.RegularExpressions;

namespace MvcDonata.Controllers
{
    public class WXController : Controller
    {
        //
        // GET: /WX/

        public ActionResult Index(string signature,string timestamp,string nonce,string echostr)
        {

            return Content(echostr);
        }




        public ActionResult test()
        {
            return Content(WX_Message.command("",""));
        }



        [HttpPost]
        public ActionResult Index()
        {
            var stream = Request.InputStream;
            var buffer = new byte[1024];
            var len = stream.Read(buffer, 0, buffer.Length);
            var str = Encoding.UTF8.GetString(buffer, 0, len);
            var start = str.IndexOf("<xml>");
            var xml = str.Substring(start);

            xml.Log(@"e:\log\wx_replay.txt");

            var e = XElement.Parse(xml);

            var me = e.Element("ToUserName").Value;
            var user = e.Element("FromUserName").Value;

            var msgType = e.Element("MsgType").Value;

            var template = @"<xml>
                            <ToUserName><![CDATA[{0}]]></ToUserName>
                            <FromUserName><![CDATA[{1}]]></FromUserName>
                            <CreateTime>12345678</CreateTime>
                            <MsgType><![CDATA[text]]></MsgType>
                            <Content><![CDATA[{2}]]></Content>
                            <FuncFlag>0</FuncFlag>
                        </xml> ";
            var replay = "";

            if (DateTime.Now > new DateTime(2013, 8, 17) && DateTime.Now < new DateTime(2013, 8, 20))
            {
                return Content(string.Format(template, user, me, "亲:多纳达于2013-08-17至2013-08-19停业整顿三天,20号开始营业,特此通知,给您带来不便请谅解!"));
            }

            switch (msgType)
            {
                case "text":
                    #region 处理文本消息

                    //如果不是上班时间,就提示客人现在是下班时间,不接受单子
//                    var startTime = new TimeSpan(10, 0, 0);
//                    var endTime = new TimeSpan(22, 30, 0);

//                    var now = DateTime.Now.TimeOfDay;

                    
//                    if (now > endTime || now < startTime)
//                    {
//                        return Content(string.Format(template, user, me, @"十分感谢您的访问.
//现在我们还没有开始营业,我们的营业时间是10:00-23:00.如果因此给您带来不便,我们万分抱歉!"));
//                    }


                    var msg = e.Element("Content").Value.Trim();


                    string.Format("MsgID:{0},Content:{1}",e.Element("MsgId").Value,msg).Log(@"e:\log\wx_replay.txt");


                    var reg = new Regex(@"^(?<class>\w)(?<page>\d{0,1})$");

                    ("[" + msg + "]").Log(@"e:\log\wx_replay.txt");

                    if (msg == "sms")
                    {
                        using (var client = new WebService.LinkWSSoapClient())
                        {
                            var count = client.SelSum("tclkj02192", "zytzly97");
                            replay = string.Format("剩余短信次数为：{0}.", count);
                        }
                        replay.Log(@"e:\log\wx_replay.txt");

                        return Content(string.Format(template, user, me, replay));
                    }

                    var mt = reg.Match(msg);

                    if (mt.Success)
                    {
                        int page = 1;
                        int.TryParse(mt.Groups["page"].Value, out page);

                        var className = mt.Groups["class"].Value.ToLower();


                        switch (className)
                        {
                            case "b":
                                replay = WX_Message.pizza(user, me, page);
                                break;
                            case "s":
                                replay = WX_Message.sandwitch(user, me, 1);
                                break;
                            case "x":
                                replay = WX_Message.template(user, me, "小吃", "snack", page, 9);
                                break;
                            case "z":
                                replay = WX_Message.template(user, me, "主食", "main", page, 9);
                                break;
                            case "t":
                                replay = WX_Message.template(user, me, "汤", "soap", page, 9);
                                break;
                            case "l":
                                replay = WX_Message.template(user, me, "沙拉", "salad", page, 9);
                                break;
                            default:
                                replay = WX_Message.command(user, me);
                                break;
                        }


                        

                        return Content(replay);
                    }
#endregion
                    break;
                case "event":
                    var ev=e.Element("Event").Value;
                    if (ev == "unsubscribe") //如果用户取消订阅,就重置时间为2小时以前
                    {
                        var t = DateTime.Now - new TimeSpan(2, 0, 0);
                        Models.BusinessObject.IsWXCommandSended(user, true);
                        replay = "";
                    }
                    else
                    {
                        replay = WX_Message.command(user, me);
                    }
                    break;


                default:
                    replay = "";
                    break;

            }
            return Content(replay);

        }
    }
}
