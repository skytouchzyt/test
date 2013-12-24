using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tools;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;

namespace MvcDonata
{



    public class WX_Message
    {
        /// <summary>
        /// 记录上一次给用户发送命令帮助的时间,防止重复给用户发送命令帮助
        /// 一小时只发送一次
        /// </summary>
        public class LastTimeCommandSended
        {
            public ObjectId _id { private get; set; }
            public string OpenID { get; set; }
            public string Time { get; set; }
        }

        public static readonly int pageSize = 5;
        public static readonly string template_article = @"<xml>
                 <ToUserName><![CDATA[{0}]]></ToUserName>
                 <FromUserName><![CDATA[{1}]]></FromUserName>
                 <CreateTime>12345678</CreateTime>
                 <MsgType><![CDATA[news]]></MsgType>
                 <ArticleCount>{2}</ArticleCount>
                 <Articles>
                    {3}                 
                 </Articles>
                 <FuncFlag>1</FuncFlag>
                 </xml> ";

        public static readonly string template_item = @"<item>
                 <Title><![CDATA[{0}]]></Title>
                 <Description><![CDATA[{1}]]></Description>
                 <PicUrl><![CDATA[{2}]]></PicUrl>
                 <Url><![CDATA[{3}]]></Url>
                 </item>";
        public static readonly string template_url = @"http://180.186.21.156/content/images/wx/{0}/{1}.jpg";



        public class dish
        {
            public string name { get; set; }
            public string imageUrl { get; set; }
        }

        public static List<dish> pizzas = new List<dish>
        {
            new dish{
                                 name=@"新德里辣牛,9寸￥39,12寸￥59.

秘制辣牛肉,青椒,花生",
                                 imageUrl="laniu1"
                    },
            new dish{
                                 name=@"BBQ鸡肉,9寸￥45,12寸￥55.

鸡肉丁,烟熏鸡肉肠,青椒,洋葱,蘑菇",
                                 imageUrl="bbq1"
                    },
                    new dish{
                        name=@"超级至尊,9寸￥39,12寸￥49.

意大利香肠,猪肉粒,牛肉粒,意大利肉粒,火腿,青椒,洋葱,蘑菇,菠萝,黑橄榄",
                        imageUrl="chaoji"
                    },
                    new dish{
                        name=@"奶香风情,9寸￥39,12寸￥49.

火腿,培根,蘑菇,圣女果,奶香酱",
                        imageUrl="naixiang1"
                    },
                    new dish{
                        name=@"珠圆玉润,9寸￥39,12寸￥49.

意大利香肠,意大利肉粒,培根,青椒,洋葱,蘑菇",
                        imageUrl="zhuyuan1"
                    },
                    new dish{
                        name=@"玛格丽特,9寸 ￥35,12寸 ￥45.

奶酪,比萨底酱",
        imageUrl="mage1"
                    },
                    new dish{
                        name=@"果木姜汁鸭胸(清真),9寸 ￥49,12寸 69.

姜汁鸭胸,鸡肉丁,青红椒,洋葱",
                imageUrl="guomu1"
                    },
                    new dish{
                        name=@"荷塘月色,9寸 ￥45,12寸 ￥55.

意大利香肠,金枪鱼,蟹肉,大虾仁,玉米,洋葱",
                       imageUrl="hetang1"
                    },
                    new dish{
                        name=@"金枪鱼特选,9寸 ￥29,12寸 ￥39.

金枪鱼,洋葱,黑橄榄,奶酪",
              imageUrl="jinqiang1"
                    },
                    new dish{
                        name=@"美式风情,9寸 ￥35,12寸 ￥45.

意大利香肠,奶酪",
         imageUrl="meishi1"
                    },
                    new dish{
                        name=@"挪威海趣(清真),9寸 ￥59,12寸 ￥79.

三文鱼,鱿鱼,QQ虾,青椒,洋葱,蘑菇",
                    imageUrl="nuowei1"
                    },
                    new dish{
                        name=@"泡菜肥牛(清真),9寸 ￥49,12寸 ￥69.

精致韩国泡菜,上选肥牛,青椒,洋葱",
                  imageUrl="paocai1"
                    },
                    new dish{
                        name=@"水果大比拼,9寸 ￥39,12寸 ￥49.

香蕉,黄桃,菠萝,苹果,椰果",
               imageUrl="shuiguo1"
                    },
                    new dish{
                        name=@"田园风光(全素),9寸 ￥29,12寸 ￥39.

蘑菇,青椒,菠萝,番茄,玉米",
               imageUrl="tianyuan1"
                    },
                    new dish{
                        name=@"香烤牛肉(清真),9寸 ￥45,12寸 ￥55.

烤牛肉,牛肉粒,青椒,洋葱,玉米",
                 imageUrl="xiangkao1"
                    }

        };

        public static List<dish> commands = new List<dish>
        {

            new dish{
                name=@"    订餐请直接语音留言,头一次使用微信的顾客请报一下电话和地址.为了送餐及时准确,如果您有多个地址,请务必每次说明.

    我们客户人员给您下单后,系统将自动把您下的订单信息通过短信发送到您的手机里,请注意查看.

    您也可以发送下面的命令查看餐品.

    发送任意其他字母,系统将回复本页说明.
",
                          imageUrl=""
            },

            new dish{
                name=@"命令 B
用于查看比萨.
发送'B1',查看第一页;
发送'B2',查看第二页;
所有其他命令以此类推,
命令不分大小写.",
             imageUrl="/pizza/naixiang.jpg"},

             new dish{
                 name=@"命令 X
用于查看小吃.",
        imageUrl="/snack/hjdpp.jpg"},

        new dish{
            name=@"命令 Z
用于查看主食(中式饭面).",
              imageUrl="/main/cxnrjym.jpg"
        },

        new dish{
            name=@"命令 L
用于查看沙拉.",
        imageUrl="/salad/qcswy.jpg"
        },

        new dish{
            name=@"命令 S
用于查看三明治.",
         imageUrl="/sandwitch/sandwitch.jpg"
        },

        new dish{
            name=@"命令 T
用于查看汤.",
       imageUrl="/soap/dyghxt.jpg"
        }
        };






        public static dish logo = new dish
        {
            name = @"多纳达比萨 {0}
第{1}页,共{2}页,点击查看大图!",
            imageUrl = "logo"
        };

        public static int calcPageCount(int total,int ps)
        {
            int size = total / ps;
            return size + (total % ps == 0 ? 0 : 1);
        }


        public static string sandwitch(string user, string me, int page,int size=0)
        {
            var list = Models.BusinessObject.GetDishesByClassName("三明治");
            var query = from dish in list
                        group dish by Models.BusinessObject.GetDishRealName(dish.Name) into g
                        select g;
            var template = @"{0} 
            6寸 {1:c},12寸 {2:c}";
            var strs = new List<string>();
            foreach (var g in query)
            {
                var small = g.Where(i => i.Name.Contains("6")).First();
                var big = g.Where(i => i.Name.Contains("12")).First();
                strs.Add(string.Format(template, g.Key.Replace("三明治",""), small.Price, big.Price));
            }
            var str = strs.JoinBy(Environment.NewLine);
            var items = "";
            items += string.Format(template_item, string.Format(logo.name,"三明治", 1, 1), "", "http://www.4008862727.com/content/images/logo.jpg", "");
            items += string.Format(template_item, str, "",
                    string.Format(template_url, "sandwitch", "sandwitch"),
                    ""
                    );

            return string.Format(template_article,
                user,
                me,
                2,
                items);

        }




        public static string template(
            string user,
            string me,
            string className,
            string imageDir,
            int page,
            int size=0
            )
        {
            //完成分页功能

            if (size == 0)
                size = pageSize;

            page = page - 1;
            page = Math.Max(page, 0);

            var dish = Models.BusinessObject.GetDishesByClassName(className);

            var list = dish.Skip(page * size).Take(size);
            var pageCount = calcPageCount(dish.Count(), size);

            page = Math.Min(page, pageCount - 1);

            if (list.Count() == 0)
            {
                page = 0;
                list = dish.Take(pageSize);
            }
            var items = "";

            items += string.Format(template_item, string.Format(logo.name, className, page + 1, pageCount), "", "http://www.4008862727.com/content/images/logo.jpg", "");



            foreach (var p in list)
            {
                var image = Models.BusinessObject.GetDishRealName(p.Name).FirstPY();
                items += string.Format(template_item, string.Format(@"{0} {1:c}", p.Name, p.Price), "",
                    string.Format(template_url, imageDir, image),
                    string.Format(template_url, imageDir, image)
                    );
            }

            return string.Format(template_article,
                user,
                me,
                list.Count() + 1,
                items);
        }


        public static string command(string user, string me)
        {

            if (Models.BusinessObject.IsWXCommandSended(user))
                return "";

            var items = "";

            items += string.Format(template_item, @"欢迎光临多纳达比萨", "", "http://www.4008862727.com/content/images/logo.jpg", "");

            foreach (var p in commands)
            {
                var image="http://www.4008862727.com/content/images/wx"+p.imageUrl;
                items += string.Format(template_item, p.name, p.name,
                    image,
                    ""
                    );
            }

            return string.Format(template_article,
                user,
                me,
                commands.Count() + 1,
                items);
        }

        public static string pizza(string user, string me, int page,int size=0)
        {



            //完成分页功能

            if (size == 0)
                size = pageSize;
            
            page=page-1;
            page=Math.Max(page,0);

            var list = pizzas.Skip(page * size).Take(size);
            var pageCount=calcPageCount(pizzas.Count(),size);

            page = Math.Min(page, pageCount-1);

            if (list.Count() == 0)
            {
                page = 0;
                list = pizzas.Take(pageSize);
            }
            var items = "";


            items += string.Format(template_item, string.Format(logo.name,"比萨饼",page+1,pageCount), "", "http://www.4008862727.com/content/images/logo.jpg", "");



            foreach (var p in list)
            {
                items += string.Format(template_item, p.name, p.name,
                    string.Format(template_url, "pizza", p.imageUrl),
                    string.Format(template_url, "pizza", p.imageUrl)
                    );
            }

            return string.Format(template_article,
                user,
                me,
                list.Count() + 1,
                items);

        }
    }
}