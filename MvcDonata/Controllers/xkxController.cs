using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tools;
using System.Text;
using System.Xml.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;

using MongoDB;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace MvcDonata.Controllers
{
    public class xkxController : Controller
    {

        public class LoginResult
        {
            public int Ret { get; set; }
            public string ErrMsg { get; set; }
            public int ShowVerifyCode { get; set; }
            public int ErrCode { get; set; }
        }
        public class SendMessageResult
        {
            public int ret { get; set; }
            public string msg { get; set; }
        }
        public class Message
        {
            public int type { get; set; }
            public string content { get; set; }
            public bool error { get; set; }
            public string imgcode { get; set; }
            public string tofakeid { get; set; }
            public string token { get; set; }
            public int ajax { get; set; }
        }

        public class ContactPageInfo
        {
            //token=610706483&t=wxm-friend&lang=zh_CN&type=0&keyword=&groupid=0&pagesize=10&pageidx=1
            public string t { get; set; }
            public string token { get; set; }
            public string lang { get; set; }
            public int type { get; set; }
            public string keyword { get; set; }
            public int groupid { get; set; }
            public int pagesize { get; set; }
            public int pageidx { get; set; }
        }

        public class Friend
        {
            public string fakeId { get; set; }
            public string nickName { get; set; }
            public string remarkName { get; set; }
            public int groupId { get; set; }
        }

        public class WX:IDisposable
        {
            private string token = "";
            private string cookie = "";
            public Dictionary<string, Friend> friends = new Dictionary<string, Friend>();
            private WebClient client = new WebClient();
            private string account;
            private string password;



            public void Login()
            {
                client.Headers.Add("Accept: */*");

                client.Headers.Add("User-Agent: Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0; .NET4.0E; .NET4.0C; InfoPath.2; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; SE 2.X MetaSr 1.0)");

                client.Headers.Add("Accept-Language: zh-cn");

                //很重要,否则登录会失败
                client.Headers.Add("Content-Type: application/x-www-form-urlencoded; charset=UTF-8");

                client.Headers.Add("Accept-Encoding: gzip, deflate,sdch");

                client.Headers.Add("Cache-Control: no-cache");

                client.Encoding = Encoding.UTF8;

                var data = new
                {
                    username = account,
                    pwd = password.MD5(),
                    imgcode = "",
                    f = "json"
                };


                var post = Encoding.UTF8.GetBytes(data.ToUrlString());

                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                client.Headers.Add("ContentLength", post.Length.ToString());

                var r = client.UploadData("https://mp.weixin.qq.com/cgi-bin/login?lang=zh_cn", "POST", post);
                var str = Encoding.UTF8.GetString(r);
                var result = str.DeserializeFromJson<LoginResult>(false);

                str.Log("e:\\xkx.txt");


                if (result.ErrCode != 0)
                    throw new Exception(result.ErrMsg);



                cookie = client.ResponseHeaders["Set-Cookie"].ToString();

                cookie = GetCookie(cookie);




                var reg = new Regex(@"token=(?<token>\d+)");
                var mt = reg.Match(result.ErrMsg);

                token = mt.Groups["token"].Value;

                client.Headers.Add(cookie);

                friends.Clear();
            }
            public WX(string acc, string pwd)
            {
                account = acc;
                password = pwd;
                Login();
                GetFriends();                
            }

         
            public Friend GetFriendByNickname(string nickname)
            {
                var query = from f in friends.Values
                            where f.nickName == nickname
                            select f;
                if (query.Count() == 0)
                    return null;
                else
                    return query.First();
            }
            public void SendMessage(string nickname, string message)
            {
                var f = GetFriendByNickname(nickname);
                if (f == null)
                    throw new Exception("没有找到此微信号,请确认此微信号是否加关注公众账号'侠客行网关'!");

                for (var i = 0; i < 3; i++) //如果失败就重复发送3次
                {
                    //重要的头
                    client.Headers.Add(string.Format("Referer:https://mp.weixin.qq.com/cgi-bin/singlemsgpage?token={0}&fromfakeid={1}&msgid=&source=&count=20&t=wxm-singlechat&lang=zh_CN",
                        token, f.fakeId
                        ));

                    //client.Headers.Add("X-Requested-With:XMLHttpRequest");


                    var post = Encoding.UTF8.GetBytes(
                        new Message
                        {
                            ajax = 1,
                            content = message,
                            error = false,
                            imgcode = "",
                            tofakeid = f.fakeId,
                            token = token,
                            type = 1
                        }.ToUrlString());

                    var r = client.UploadData("https://mp.weixin.qq.com/cgi-bin/singlesend?t=ajax-response&lang=zh_CN",
                        "POST",
                        post
                        );
                    var result = Encoding.UTF8.GetString(r).DeserializeFromJson<SendMessageResult>(false);
                    if (result.ret == 0)
                        break;
                    Login();
                    GetFriends();
                }
            }
            public void GetFriends()
            {
                int pageCount = 0;
                var pageIndex = 0;

                var list = GetFriendsByPage(pageIndex, out pageCount);
                while (pageIndex < pageCount)
                {
                    pageIndex++;

                    foreach (var f in list)
                    {
                        if(!friends.ContainsKey(f.fakeId))
                            friends.Add(f.fakeId, f);
                    }

                    list = GetFriendsByPage(pageIndex, out pageCount);
                }
            }

            private List<Friend> GetFriendsByPage(int pageIndex, out int pageCount)
            {
                var info = new ContactPageInfo
                {
                    groupid = 0,
                    keyword = "",
                    lang = "zh_CN",
                    pageidx = pageIndex,
                    pagesize = 10,
                    token = token,
                    type = 0,
                    t = "wxm-friend"
                };

                var url = "https://mp.weixin.qq.com/cgi-bin/contactmanagepage";
                var dataurl = info.ToUrlString();
                url += "?" + dataurl;

                client.Headers.Remove("Referer");
                client.Headers.Add("Referer:" + url);

                var result = Encoding.UTF8.GetString(client.DownloadData(url));

                var str = "<script id=\"json-friendList\" type=\"json/text\">";
                var begin = result.IndexOf(str);

                var r = result.Substring(begin + str.Length);

                var end = r.IndexOf("</script>");

                r = r.Substring(0, end);

                var reg = new Regex(@"PageCount\s+:\s+'(?<pageCount>\d+)'\*1");
                var mt = reg.Match(result);
                if (mt.Success)
                {
                    pageCount = int.Parse(mt.Groups["pageCount"].Value);
                }
                else
                    pageCount = 1;


                return r.DeserializeFromJson<List<Friend>>(false);
            }

            private string GetCookie(string CookieStr)
            {

                string result = "";



                string[] myArray = CookieStr.Split(',');

                if (myArray.Count() > 0)
                {

                    result = "Cookie: ";

                    foreach (var str in myArray)
                    {

                        string[] CookieArray = str.Split(';');

                        result += CookieArray[0].Trim();

                        result += "; ";

                    }

                    result = result.Substring(0, result.Length - 2);

                }

                return result;

            }
            public void Dispose()
            {
                
            }
        }

        //
        // GET: /xkx/

        public ActionResult Index(string signature, string timestamp, string nonce, string echostr)
        {
            return Content(echostr);
        }

        [HttpPost]
        public ActionResult SendMessage(string nickname, string message)
        {
            try
            {
                var app = HttpContext.Application;
                app.Lock();
                var wx = app["wx"] as WX;
                app.UnLock();
                if (wx == null)
                {
                    wx = new WX("zyt97711@sina.com", "zytzly97");
                    app.Lock();
                    app["wx"] = wx;
                    app.UnLock();
                }
                wx.SendMessage(nickname, message);
                return Content("发送成功.");
            }
            catch (Exception e)
            {
                return Content("服务器错误:"+e.Message);
            }
        }

        [HttpGet]
        public ActionResult GetMessage()
        {
            var list = GetMessages();
            return Content(string.Join("\n", list));
        }

        public ActionResult list()
        {

            var app = HttpContext.Application;
            app.Lock();
            var wx = app["wx"] as WX;
            app.UnLock();

            if (wx != null)
            {
                return Json(wx.friends.Values, JsonRequestBehavior.AllowGet);
            }
            return Content("");
        }
        private static string template = @"<xml>
                            <ToUserName><![CDATA[{0}]]></ToUserName>
                            <FromUserName><![CDATA[{1}]]></FromUserName>
                            <CreateTime>12345678</CreateTime>
                            <MsgType><![CDATA[text]]></MsgType>
                            <Content><![CDATA[{2}]]></Content>
                            <FuncFlag>0</FuncFlag>
                        </xml> ";


        private List<string> GetMessages()
        {
            
            var app = HttpContext.Application;
            app.Lock();
            try
            {
                var list = app["list"] as List<string>;
                if (list == null)
                {
                    list = new List<string>();
                    app["list"] = list;
                }
                app["list"] = new List<string>();
                return list.ToList();
            }
            finally
            {
                app.UnLock();
            }

        }
        private void AddMessage(string message)
        {
            var app = HttpContext.Application;
            app.Lock();
            try
            {
                var list = app["list"] as List<string>;
                if (list == null)
                {
                    list = new List<string>();
                    app["list"] = list;
                }
                list.Add(message);
            }
            finally
            {
                app.UnLock();
            }
        }

        [HttpPost]
        public ActionResult Index()
        {
            var user="";
            var me="";
            var replay = "";
            try
            {

                var app = HttpContext.Application;
                app.Lock();
                var wx = app["wx"] as WX;
                app.UnLock();

                if (wx == null)
                {
                    try
                    {

                        "wx is null".Log("e:\\xkx.txt");
                        wx = new WX("zyt97711@sina.com", "zytzly97");
                        app.Lock();
                        app.Add("wx", wx);
                        app.UnLock();
                    }
                    catch (Exception ex)
                    {
                        ex.Message.Log("e:\\xkx.txt");
                    }
                }


                var stream = Request.InputStream;
                var buffer = new byte[1024];
                var len = stream.Read(buffer, 0, buffer.Length);
                var str = Encoding.UTF8.GetString(buffer, 0, len);
                var start = str.IndexOf("<xml>");
                var xml = str.Substring(start);



                var e = XElement.Parse(xml);

                me = e.Element("ToUserName").Value;
                user = e.Element("FromUserName").Value;

                var msgType = e.Element("MsgType").Value;

                

                var helpmessage = @"欢迎您访问侠客行微信网关!使用方法:和xkx中的tell命令一样,tell somebody something.在xkx中,somebody为你的微信号,在微信中somebody为你的xkx id.例如:在xkx中给你的微信发信息,使用命令tell weixin 你的微信号 hello,这样你的微信就能收到hello了.weixin是我在xkx sz站的id,你需要通过这个id发送和接受信息.在微信中使用命令tell xkx_id hello.我的id就会通过tell发送hello给你的id.注意了:你的微信号不能有空格!!!";

                switch (msgType)
                {
                    case "text":

                        var args = e.Element("Content").Value.Trim().Split(' ');

                        if (args.Length < 3)
                        {
                            throw new Exception("请使用格式:tell xkxid somthing.");
                        }
                        var msg = string.Join(" ", args, 2, args.Length - 2);
                        msg = string.Format("tell {0} {1}\n", args[1], msg);
                        AddMessage(msg);

                        break;
                    case "event":
                        var ev = e.Element("Event").Value;
                        if (ev == "subscribe")
                        {
                            replay = string.Format(template, user, me, helpmessage);
                            wx.GetFriends();
                        }
                        break;


                    default:
                        replay = "";
                        break;

                }

            }
            catch (Exception exe)
            {
                replay = string.Format(template, User, me, exe.Message);
            }
            return Content(replay);

        }

    }
}
