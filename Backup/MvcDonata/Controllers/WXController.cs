using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Tools;
using System.Xml.Linq;
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

        [HttpPost]
        public ActionResult Index()
        {
            var stream = Request.InputStream;
            var buffer=new byte[1024];
            var len = stream.Read(buffer, 0, buffer.Length);
            var str = Encoding.UTF8.GetString(buffer, 0, len);
            var start = str.IndexOf("<xml>");
            var xml = str.Substring(start);
            var e = XElement.Parse(xml);

            var me=e.Element("ToUserName").Value;
            var user = e.Element("FromUserName").Value;
            var msg = e.Element("Content").Value;

            var template=@"<xml>
                            <ToUserName><![CDATA[{0}]]></ToUserName>
                            <FromUserName><![CDATA[{1}]]></FromUserName>
                            <CreateTime>12345678</CreateTime>
                            <MsgType><![CDATA[text]]></MsgType>
                            <Content><![CDATA[{2}]]></Content>
                            <FuncFlag>0</FuncFlag>
                        </xml> ";


            var replay = string.Format("欢迎光临多纳达,请直接语音留言即可订餐!头一次使用微信点餐的客户请报一下联系电话。");

            return Content(string.Format(template,user,me,replay));
        }
    }
}
