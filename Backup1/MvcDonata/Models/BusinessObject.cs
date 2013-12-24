using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Tools;
using System.Transactions;
using System.Text;
using System.Data.Objects;
using System.IO;
using System.Runtime.CompilerServices;

using Models.MongoDB;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;

namespace MvcDonata.Models
{

    public class BusinessObject
    {
        public static string Version
        {
            get { return "v2013-10-18 09:32"; }
        }
        public static string mongodbAddress;
        public static readonly string datetimeFormatString = "yyyy-MM-dd HH:mm:ss";


        public static decimal CalcTotalPhoneOrders(string node)
        {
            var db = BusinessObject.mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<PhoneOrder>("PhoneOrders");
            var start = DateTime.Now.Date.ToString(BusinessObject.datetimeFormatString);
            var query = collection.Find(Query.GTE("CreatedTime", start)).AsEnumerable();
            query=query.Where(i =>
            {
                return DateTime.Parse(i.CreatedTime) >= DateTime.Parse(start)&&i.Node==node;
            }
            );
            return query.Sum(i => i.Total);
        }
        public static decimal CalcTotalDineOrders(string node)
        {
            var db = BusinessObject.mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<DineOrder>("DineOrders");
            var start = DateTime.Now.Date.ToString(BusinessObject.datetimeFormatString);
            var query = collection.Find(Query.GTE("Time", start)).AsEnumerable();
            query=query.Where(i =>
                {
                    return DateTime.Parse(i.Time) >= DateTime.Parse(start)&&i.Node==node;
                }
            );
            return query.Sum(i => i.Total);
        }

        public static void UpsertDNS(DNS dns)
        {
            var db = mongodbAddress.MongoDB("donata");
            var collecton = db.GetCollection<DNS>("DNSes");
            var query = from d in collecton.AsQueryable()
                        where d.Node==dns.Node
                        select d;
            if (query.Count() == 0)
            {
                collecton.Insert(dns, SafeMode.True);
            }
            else
            {
                dns._id = query.First()._id;
                var r = CreateMongoUpdate(dns, "Node", dns.Node);
                collecton.Update(r.Item2, r.Item1, SafeMode.True);
            }
        }
        public static List<DNS> GetDNSes()
        {
            var db = mongodbAddress.MongoDB("donata");
            var collecton = db.GetCollection<DNS>("DNSes");
            var start = (DateTime.Now - new TimeSpan(1, 0, 0, 0)).ToString(datetimeFormatString);
            var query = Query.GTE("LastUpdatedTime", start);

            return collecton.Find(query).ToList();
        }
        
        public static List<Dish> GetDishesByClassName(string className)
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<Dish>("Dishes");
            var query = from d in collection.AsQueryable()
                        where d.ClassName == className
                        select d;
            return query.ToList();
        }

        #region 餐品操作
        public static string GetDishRealName(string name)
        {
            var sb = new StringBuilder();
            foreach (var c in name)
            {
                if (c >= '0' && c <= '9')
                    break;
                if (c == '(')
                    break;
                if (c == ',')
                    break;
                sb.Append(c);
            }
            return sb.ToString();
        }
        public static string GetImageFileName(Dish dish)
        {

            if (dish.ClassName == "三明治")
            {
                if (dish.Name.Contains("6"))
                    return dish.ClassName + "6";
                else
                    return  dish.ClassName + "12";
            }

            return GetDishRealName(dish.Name);

        }



        private static string GetNormalName(string name)
        {
            var temp=name.Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries);
            return temp[0];
        }
        private static string GetPrintingName(string name)
        {
            var temp = name.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length == 1)
                return temp[0];
            else
                return temp[1];
        }

        /// <summary>
        /// 获取激活的餐品,并且计算30天餐品出售的次数
        /// 客户端以此作为热卖度,来排序餐品
        /// </summary>
        /// <param name="dine">计算堂食或者计算外卖</param>
        /// <returns></returns>
        public static List<Dish> GetDishesWithCount(bool dine=true)
        {
            var db = mongodbAddress.MongoDB("donata");
            var dineorders = db.GetCollection<DineOrder>("DineOrders");
            var phoneorders = db.GetCollection<PhoneOrder>("PhoneOrders");
            var start = (DateTime.Now.Date - new TimeSpan(30, 0, 0, 0)).ToString(datetimeFormatString);
            var query=Query.GTE("Time", start);
            var dines = dineorders.Find(query);
            var phones = phoneorders.Find(query);
            var dishes=GetDishes();

            if (dine)
            {
                foreach (var o in dines)
                {

                    foreach (var d in o.Details)
                    {
                        foreach (var dish in dishes)
                        {
                            if (dish.Name == d.DishName)
                            {
                                dish.Count += d.Amount;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var o in phones)
                {

                    foreach (var d in o.Details)
                    {
                        foreach (var dish in dishes)
                        {
                            if (dish.Name == d.DishName)
                            {
                                dish.Count += d.Amount;
                                break;
                            }
                        }
                    }
                }
            }
            return dishes;
        }

        /// <summary>
        /// 获取所有的餐品
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public static List<Dish> GetDishes(bool all=false)
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<Dish>("Dishes");

            var allDishes = (from dish in collection.AsQueryable()
                        select dish).ToList();
            var query = from dish in allDishes
                        where dish.Actived || all
                        select dish;
            return query.ToList();
        }

        /// <summary>
        /// 检测一小时内是否发送过命令帮助
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static bool IsWXCommandSended(string openid,bool reseted=false)
        {

            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<WX_Message.LastTimeCommandSended>("LastTimeCommandSended");
            var query = from i in collection.AsQueryable()
                        where i.OpenID == openid
                        select i;

            if (!reseted)
            {
                if (query.Count() == 0)
                {
                    collection.Insert(new WX_Message.LastTimeCommandSended
                    {
                        OpenID = openid,
                        Time = DateTime.Now.ToString(datetimeFormatString)
                    }, SafeMode.True);
                    return false;
                }
                var last = DateTime.Parse(query.First().Time);
                var span = (DateTime.Now - last).TotalHours;
                if (span < 1)
                {
                    return true;
                }
                var target = query.First();
                target.Time = DateTime.Now.ToString(datetimeFormatString);
                var result = CreateMongoUpdate(target, "OpenID", openid);
                collection.Update(result.Item2, result.Item1, SafeMode.True);
                return false;
            }
            else //否则写入指定的时间
            {
                var target = query.First();
                target.Time = (DateTime.Now-new TimeSpan(2,0,0)).ToString(datetimeFormatString);
                var result = CreateMongoUpdate(target, "OpenID", openid);
                collection.Update(result.Item2, result.Item1, SafeMode.True);
                return false;
            }
        }

        

        public static void SetSoldOutDish(string node, string dishName, bool soldOut)
        {
            
        }




        public static void UpdateDish(Dish dish)
        {

            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<Dish>("Dishes");
            var query = from ds in collection.AsQueryable()
                        where ds.Name == dish.Name
                        select ds;
            if (query.Count()== 0)
                throw new Exception("没有找到此餐品");
            var target = query.First();
            dish._id = target._id;
            var result = CreateMongoUpdate(dish, "Name", dish.Name);


            collection.Update(result.Item2, result.Item1, SafeMode.True);
        }


        public static void UpdateInsertDishes(IEnumerable<Dish> dishes)
        {

            foreach (var d in dishes)
            {
                //首先看是否已经有此餐品
                //如果有就更新
                //没有就插入
                var db = mongodbAddress.MongoDB("donata");
                var collection = db.GetCollection<Dish>("Dishes");
                var query = from ds in collection.AsQueryable()
                            where ds.Name == d.Name
                            select ds;
               
                if (query.Count() == 0)
                {
                    collection.Insert(d, SafeMode.True);
                }
                else
                {
                    UpdateDish(d);
                }
            }



        }



        public static List<string> GetBars()
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<Dish>("Dishes");
            var query = from d in collection.AsQueryable()
                        select d.DefaultBar;
            var list = new List<string>();

            query.Distinct().ToList().ForEach(i => list.AddRange(i.Split(new char[] { ',' })));
            return list.Distinct().ToList();
        }

        public static void DeleteDish(int id)
        {
            
        }

        #endregion

        #region 顾客操作









      

   


   
      


      
        /// <summary>
        /// 更新或者插入顾客
        /// </summary>
        /// <param name="customer"></param>
        public static void UpdateInsertCustomer(Customer customer)
        {

            try
            {
                
                var db = mongodbAddress.MongoDB("donata");
                var collection = db.GetCollection<Customer>("Customers");

                var query = from c in collection.AsQueryable()
                            where c.Phone == customer.Phone
                            select c;

                if (query.Count() == 0)
                {
                    collection.Insert(customer, SafeMode.True);
                }
                else
                {

                    BsonDocument bd = BsonExtensionMethods.ToBsonDocument(customer);

                    IMongoQuery qd = Query.EQ("Phone", customer.Phone);

                    collection.Update(qd, new UpdateDocument(bd), SafeMode.True);
                   
                }
            }
            catch
            {

            }

        }
    


        #endregion

        #region 电话订单操作

        public static void NewPhoneCall(string number,string phone, DateTime time)
        {

            var call = new CallRecord
            {
                Phone = phone,
                Time = time.ToString(datetimeFormatString),
                BarNumber=number,
                Received=false
            };
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<CallRecord>("CallRecords");

           collection.Insert(call, SafeMode.True);
            

        }


        /// <summary>
        /// 获取新呼入的电话,只获取30秒以内的
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string GetNewPhoneCall(string number)
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<CallRecord>("CallRecords");
            var qtime = Query.GTE("Time", (DateTime.Now - new TimeSpan(0, 0, 30)).ToString(datetimeFormatString));
            var qnumber = Query.EQ("BarNumber", number);
            var qreceived = Query.EQ("Received", false);
            var qd = Query.And(qtime, qnumber, qreceived);
            var query = collection.Find(qd);
            if (query.Count() > 0)
            {
                var c = query.First();
                var qid = Query.EQ("_id", new BsonObjectId(c.ID));
                c.Received = true;
                var bd = BsonExtensionMethods.ToBsonDocument(c);

                collection.Update(qid, new UpdateDocument(bd), SafeMode.True);

                return c.Phone;
            }
            else
            {
                return "";
            }


        }



        public static void InsertPhoneOrder(List<PhoneOrder> list)
        {

        }

        /// <summary>
        /// 分店通过此函数获取新的没有接收到的外送订单
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static List<PhoneOrder> GetNewPhoneOrders(string node)
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<PhoneOrder>("PhoneOrders");
            var queryTime = Query.GTE("CreatedTime", (DateTime.Now - new TimeSpan(24, 0, 0)).ToString(datetimeFormatString));
            var queryNode = Query.EQ("Node", node);

            var query = Query.And(queryTime, queryNode);

            //如果接收时间为空,就认定此订单没有被分店收到
            return collection.Find(query).Where(i=>string.IsNullOrEmpty(i.ReceivedTime)).ToList();
        }

        /// <summary>
        /// 分店通过此函数通知服务器此订单已经收到,或者通知本地服务器此订单已经被打印过
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        public static void SignPhoneOrder(List<string> barcodes,bool printed=false)
        {

            //写入MongoDB数据库
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<PhoneOrder>("PhoneOrders");
            foreach (var barcode in barcodes)
            {
                var q = from o in collection.AsQueryable()
                        where o.BarCode == barcode && string.IsNullOrEmpty(o.ReceivedTime)
                        select o;
                if (q.Count() == 0)
                    continue;
                var order = q.First();
                if (printed == false)
                    order.ReceivedTime = DateTime.Now.ToString(datetimeFormatString);
                else
                    order.Printed = true;
                var result = CreateMongoUpdate(order, "BarCode", order.BarCode);
                collection.Update(result.Item2, result.Item1, SafeMode.True);
            }

        }



        public static void SignDineOrders(IEnumerable<DineOrder> list)
        {
            //写入MongoDB数据库
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<DineOrder>("DineOrders");
            foreach (var o in list)
            {
                var query = from d in collection.AsQueryable()
                            where d.BarCode == o.BarCode && d.Time == o.Time&&d.Uploaded==false
                            select d;
                if (query.Count() == 0)
                    continue;
                var target = query.First();
                target.Uploaded = true;
                var q1 = Query.EQ("BarCode", o.BarCode);
                var q2 = Query.EQ("Time", o.Time);
                var q = Query.And(q1, q2);
                collection.Update(q, new UpdateDocument(BsonExtensionMethods.ToBsonDocument(target)),SafeMode.True);
            }
        }



        /// <summary>
        /// 插入外卖订单到数据库中
        /// </summary>
        /// <param name="po"></param>
        /// <returns>如果数据库中已经有此订单就放弃插入并且返回false,否则返回true</returns>
        public static bool InsertPhoneOrder(PhoneOrder po)
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<PhoneOrder>("PhoneOrders");
            var query = from p in collection.AsQueryable()
                        where p.BarCode == po.BarCode
                        select p;
            if (query.Count() > 0)
            {
                var temp = query.First();
                return !temp.Printed;
            }
            collection.Insert(po, SafeMode.True);
            return true;
        }

        private static string CreateNewPhoneOrderBarCode()
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<PhoneOrder>("PhoneOrders");
            //产生新的单号BarCode
            var query = from p in collection.AsQueryable()
                        select p.BarCode;
            var str = query.Max();
            var count = 198003; //默认从字数开始计数
            if (!string.IsNullOrEmpty(str))
                count = int.Parse(str.Substring(1)) + 1;
            return string.Format("P{0:D8}", count);
        }

        public static string CreateNewDineOrderBarCode(string node)
        {
            var start = DateTime.Now.Date.ToString(datetimeFormatString);
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<PhoneOrder>("DineOrders");
            var q1 = Query.GTE("Time", start);
            var q2 = Query.EQ("Node", node);
            var q = Query.And(q1, q2);
            var query = collection.Find(q);
            var index = query.Count()+1;
            return string.Format("D{0}{1:d2}{2:d2}{3:d3}", node.FirstPY().ToUpper(), DateTime.Now.Month, DateTime.Now.Day, index);
        }

        /// <summary>
        /// 呼叫中心上传新的外送订单,新的订单没有BarCode,需要此函数完成计算
        /// </summary>
        /// <param name="po"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void UploadPhoneOrder(PhoneOrder po)
        {


            var db = mongodbAddress.MongoDB("donata");

            po.CreatedTime = DateTime.Now.ToString(datetimeFormatString);
            po.BarCode = CreateNewPhoneOrderBarCode();

            InsertPhoneOrder(po);//写入数据库

            var customers = db.GetCollection<Customer>("Customers");
            //更新客户地址
            var findc=db.GetCollection<Customer>("Customers").Find(Query.EQ("Phone", po.Phone));
            Customer customer;
            if (findc.Count() > 0) //老客户
            {
                customer = findc.First();
                var have = from a in customer.Addresses
                           where a.Where == po.Address
                           select a;
                if (have.Count() == 0)
                {
                    customer.Addresses.Add(new Address
                    {
                        Where = po.Address,
                        City = "北京",
                        LastUsedTime = DateTime.Now.ToString(datetimeFormatString)
                    }
                    );
                }
            }
            else //如果是新客户就创建
            {
                customer = new Customer
                {
                    Phone = po.Phone,
                    CreatedTime = DateTime.Now.ToString(datetimeFormatString),
                    Addresses = new List<Address> { new Address { Where = po.Address, LastUsedTime = DateTime.Now.ToString(datetimeFormatString) } }
                };
                customers.Insert(customer,SafeMode.True);
            }

            //如果有VIP独享餐品就需要写入Customer.LastWeeklyDiscounted字段，防止客户重复点餐
            var find = from d in po.Details
                       where d.DishName == "VIP独享"
                       select d;
            if (find.Count() > 0)
            {
                customer.LastWeeklyDiscounted = DateTime.Now.ToString(datetimeFormatString);          
            }
            SaveCustomer(customer);

            //如果分店是test就不需要发送短信
            //if (string.Compare(po.Node, "test", true) == 0)
            //    return;


            //如果不是手机号就不需要发送短信
            if (po.Phone.Length != 11)
                return;



            //检测是否有退单，如果有退单就不发送短信
            var disorderCount = (from d in po.Details
                                 where d.Amount < 0
                                 select d
                                    ).Count();
            if (disorderCount > 0)
                return;

            //早上10:00到晚上11:00才发送短信，防止骚扰客户
            var start = new TimeSpan(10, 0, 0);
            var end = new TimeSpan(23, 0, 0);
            var now = DateTime.Now.TimeOfDay;
            if ((now <= start || now >= end) && po.Node != "test") //测试分店不受时间限制
                return;


            try
            {

                using (var client = new WebService.LinkWSSoapClient())
                {
                    var sms = "亲，您订的餐品是：{0},共计：{1}元。送餐地址:{2},请确认!";

                    if (po.OrderType == "微信")
                        sms += "现在微信可以点外卖了，并且免外送费，请用微信查找公众账号“多纳达比萨”。(勿回)";



                    


                    var details = new List<string>();
                    foreach (var d in po.Details)
                    {
                        if (string.IsNullOrEmpty(d.Remark))
                            details.Add(string.Format("{0}({1}份)", d.DishName, d.Amount));
                        else
                            details.Add(string.Format("{0}({1})({2}份)", d.DishName, d.Remark, d.Amount));
                    }

                    sms = string.Format(sms, details.JoinByChar(','), po.Total, po.Address);

                    sms.Log(@"e:\log\sms.txt");

                    client.Send("tclkj02192", "zytzly97", po.Phone, sms, "", "");



                }
            }
            catch (Exception e)
            {
                e.Message.Log(@"e:\log\sms.txt");
            }


        }
        #endregion

        #region 堂食订单操作



        /// <summary>
        /// 获取需要上传的堂食订单,只获取24小时内的订单
        /// </summary>
        /// <returns></returns>
        public static List<DineOrder> GetNeededUploadDineOrder()
        {

            var start = (DateTime.Now.Date - new TimeSpan(1, 0, 0, 0, 0)).ToString(datetimeFormatString);
            var q1 = Query.GTE("Time", start);
            var q2 = Query.EQ("Uploaded", false);
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<DineOrder>("DineOrders");
            var query = collection.Find(Query.And(q1, q2));
            return query.ToList();
        }


        public static void InsertDineOrder(List<DineOrder> orders)
        {

            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<DineOrder>("DineOrders");
            foreach (var o in orders)
            {
                //首先查询是否已经写入到数据库
                //如果是的话就不需要重复写入
                var query = from d in collection.AsQueryable()
                            where d.Time == o.Time
                            where d.BarCode == o.BarCode
                            select d;

                if (query.Count() == 0)
                {
                    collection.Insert(o, SafeMode.True);
                }
            }
        }
        #endregion


        public static void InsertNewVIP(string phone, string number,string node)
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<Customer>("Customers");

            //首先查询VIP是否已经创建过
            var query = from i in collection.AsQueryable()
                        where  i.VIP_Number == number
                        select i;
            if (query.Count() > 0)
                throw new Exception("此VIP已经创建过!");


            query = from i in collection.AsQueryable()
                        where i.Phone == phone
                        select i;

            if (query.Count() > 0)
            {
                if (!string.IsNullOrEmpty(query.First().VIP_Number))
                    throw new Exception(string.Format("此电话号码已经是VIP客户：{0}！", query.First().VIP_Number));
                else //直接写入VIP号码
                {
                    var target = query.First();
                    target.VIP_Number = number;
                    BsonDocument bd = BsonExtensionMethods.ToBsonDocument(target);

                    IMongoQuery q = Query.EQ("Phone", target.Phone);

                    collection.Update(q, new UpdateDocument(bd), SafeMode.True);
                }
            }
            else
            {

                var c = new Customer();
                c.Phone = phone;
                c.VIP_Number = number;
                c.Node = node;
                c.CreatedTime = DateTime.Now.ToString(datetimeFormatString);

                collection.Insert(c, SafeMode.True);
            }
        }

        public static Customer GetCustomerByNumber(string number)
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<Customer>("Customers");
            var query = from i in collection.AsQueryable()
                        where i.VIP_Number == number
                        select i;
            if (query.Count() == 0)
                return null;
            else
                return query.First();
        }

        public static Customer GetCustomerByPhone(string phone)
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<Customer>("Customers");

            if (phone.Length == 11||phone.Length==8)
            {


                var query = from i in collection.AsQueryable()
                            where i.Phone == phone
                            select i;
                if (query.Count() == 0)
                    return null;
                else
                    return query.First();
            }
            else if(phone.Length==6)
            {
                var query = from i in collection.AsQueryable()
                            where i.VIP_Number == phone
                            select i;
                if(query.Count()==0)
                    return null;
                else
                    return query.First();
            }
            return null;
        }


        public static void SaveCustomer(Customer customer,string newPhone="")
        {
            var oldphone = customer.Phone;
            if(string.IsNullOrEmpty(newPhone))
                newPhone=customer.Phone;
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<Customer>("Customers");

            var query = from i in collection.AsQueryable()
                        where i.Phone == customer.Phone
                        select i;
            if (query.Count() == 0)
                throw new Exception("没有找到此客户！");

            var target = query.First();
            target.Phone = newPhone;
            target.Addresses = customer.Addresses;
            target.LastBirthdayDiscounted = customer.LastBirthdayDiscounted;


            var result = CreateMongoUpdate(target, "Phone", oldphone);


            collection.Update(result.Item2, result.Item1, SafeMode.True);
        }


        public static int Charge(string phone, string number, int value)
        {
            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<Customer>("Customers");
            var query = from i in collection.AsQueryable()
                        where i.Phone == phone && i.VIP_Number==number
                        select i;
            if (query.Count() == 0)
                throw new Exception("没有找到此VIP客户！");
            var c = query.First();
            c.ChargeRecords.Add(new ChargeRecord
            {
                Time = DateTime.Now.ToString(datetimeFormatString),
                Value = value
            });

            var result = CreateMongoUpdate(c, "Phone", c.Phone);


           collection.Update(result.Item2, result.Item1,SafeMode.True);

           return value;
        }

        public static Tuple<UpdateDocument,IMongoQuery> CreateMongoUpdate(object data, string queryField, string queryFieldValue)
        {
            BsonDocument bd = BsonExtensionMethods.ToBsonDocument(data);

            IMongoQuery query = Query.EQ(queryField, queryFieldValue);

            return new Tuple<UpdateDocument, IMongoQuery>(new UpdateDocument(bd), query);
        }

        /// <summary>
        /// 取得所有的分店列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetNodes()
        {
            //通过获取30天内的外送和堂食单子来取得所有的分店
            var start=(DateTime.Now.Date-new TimeSpan(30,0,0,0)).ToString(datetimeFormatString);
            var db = mongodbAddress.MongoDB("donata");
            var cpo = db.GetCollection<PhoneOrder>("PhoneOrders");
            var cdo = db.GetCollection<DineOrder>("DineOrders");
            var qd=Query.GTE("CreatedTime",start);
            var queryP = (from p in cpo.Find(qd)
                         select p.Node).Distinct();
            qd = Query.GTE("Time", start);
            var queryD = (from d in cdo.Find(qd)
                         select d.Node).Distinct();
            return queryP.Union(queryD).Distinct().ToList();

        }


        public static Tuple<int, List<PhoneOrder>, int> GetPhoneOrders(
            bool paged,
            int pageSize,
            int pageIndex,
            string node,
            DateTime selectedDate,
            bool isAdmin = false//如果是管理员就显示test分店的单子
            )
        {
            //string.Format("isAdmin={0}", isAdmin).Log(@"e:\log\GetPhoneorders.txt");
            int pageCount = 0;
            int total = 0;

            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<PhoneOrder>("PhoneOrders");
            
            var start = selectedDate.Date.ToString("yyyy-MM-dd");
            var end = (selectedDate.Date + new TimeSpan(1, 0, 0, 0)).Date.ToString("yyyy-MM-dd");

            var bd = new BsonDocument();
            bd.Add("$gt", start);
            bd.Add("$lt", end);
            var qd = new QueryDocument();
            qd.Add("CreatedTime", bd);



            var query = from order in collection.Find(qd).AsQueryable()
                        select order;

            if (!isAdmin)
            {
                query = query.Where(i => i.Node != "test");
            }
            if (node != "全部")
            {
                query = query
                    .Where(i => i.Node == node);
                    
            }
            query = query.OrderByDescending(i => i.CreatedTime);
            if (paged)
            {
                if (pageSize != 0)
                {
                    total = query.Count();
                    pageCount = (int)Math.Ceiling(((float)total / pageSize));
                }
                pageIndex = Math.Min(pageCount - 1, pageIndex);
                pageIndex = Math.Max(0, pageIndex);
                query = query.Skip(pageSize * pageIndex).Take(pageSize);
            }

            var list = query.ToList();
            //计算实际总额 总额*打折 然后四舍五入
            list.ForEach(item =>
                {
                    item.Total = string.Compare(item.CreatedTime ,TurnningPoint.ToString(datetimeFormatString))<0 ? (item.Total * item.Discount / 10).Rounding() : item.Total;
                }
            );

            return new Tuple<int, List<PhoneOrder>, int>(pageCount, list, total);
        }



        /// <summary>
        /// 此日期以前的订单总计需要计算得出,以后的总计直接读取数据库
        /// </summary>
        static DateTime TurnningPoint = new DateTime(2012, 11, 24);


     


        public static object GetDineOrders(
            bool paged,
            int pageSize,
            int pageIndex,
            string node,
            DateTime selectedDate
            )
        {
            int pageCount=0;


            var db = mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<DineOrder>("DineOrders");

            var start = selectedDate.Date.ToString("yyyy-MM-dd");
            var end = (selectedDate.Date + new TimeSpan(1, 0, 0, 0)).Date.ToString("yyyy-MM-dd");

            var bd = new BsonDocument();
            bd.Add("$gt", start);
            bd.Add("$lt", end);
            var qd = new QueryDocument();
            qd.Add("Time", bd);

     

            var query = (from order in collection.Find(qd).AsQueryable()
                         orderby order.Time descending
                         select order).ToList();

            if (node != "全部")
            {
                query = query.Where(i => i.Node == node).ToList();
            }
            if (paged)
            {
                if (pageSize != 0)
                {
                    var total = query.Count();
                    pageCount=(int)Math.Ceiling(((float)total/pageSize));
                }
                pageIndex = Math.Min(pageCount - 1, pageIndex);
                pageIndex = Math.Max(0, pageIndex);
                query = query.Skip(pageSize * pageIndex).Take(pageSize).ToList();
            }


             return new
                    {
                        pageCount = pageCount,
                        datas = query.ToList(),
                        nodes = GetNodes(),
                        pageIndex = pageIndex
                    };

           
        }

        



        public static object GetReportDatas(DateTime start, DateTime end)
        {
            var db = mongodbAddress.MongoDB("donata");
            var cpo = db.GetCollection<PhoneOrder>("PhoneOrders");
            var cdo = db.GetCollection<DineOrder>("DineOrders");

            var c1 = Query.GTE("CreatedTime", start.ToString(datetimeFormatString));
            var c2 = Query.LTE("CreatedTime", end.ToString(datetimeFormatString));
            
            var query = Query.And(c1, c2);

            var phoneOrders = cpo.Find(query).ToList();

            c1 = Query.GTE("Time", start.ToString(datetimeFormatString));
            c2 = Query.LTE("Time", end.ToString(datetimeFormatString));
            query = Query.And(c1, c2);
            var dineOrders = cdo.Find(query).ToList();

            var groupPhone = from o in phoneOrders
                             group o by new { DateTime.Parse(o.CreatedTime).Date, o.Node } into g
                             orderby g.Key.Node, g.Key.Date
                             select new
                             {
                                 Node = g.Key.Node,
                                 IndexOfWeek = (int)g.Key.Date.DayOfWeek,
                                 Total = g.Sum(item => DateTime.Parse(item.CreatedTime) < TurnningPoint ? (item.Total * item.Discount / 10).Rounding() : item.Total),
                                 Date = g.Key.Date.Day
                             };

            var groupDine = from o in dineOrders
                            group o by new { DateTime.Parse(o.Time).Date, o.Node } into g
                            orderby g.Key.Node, g.Key.Date
                            select new
                            {
                                Node = g.Key.Node,
                                IndexOfWeek = (int)g.Key.Date.DayOfWeek,
                                Total = g.Sum(item => DateTime.Parse(item.Time) < TurnningPoint ? (item.Total * item.Discount / 10).Rounding() : item.Total),
                                Date = g.Key.Date.Day
                            };

            var nodesDine = (from o in dineOrders
                             select o.Node).Distinct();
            var nodesPhone = (from o in phoneOrders
                              select o.Node).Distinct();

            var nodes = nodesDine.Union(nodesPhone).Distinct();

            return new
            {
                DineOrders = groupDine.ToList(),
                PhoneOrders = groupPhone.ToList(),
                NodesList = nodes,
                Start = start.ToString("yyyy-MM-dd"),
                End = (end - new TimeSpan(1, 0, 0, 0)).ToString("yyyy-MM-dd")
            };


        }

        public static object GetWeeklyData(DateTime date)
        {
            var dayIndex = (int)date.DayOfWeek;
            var firstDay = (date - new TimeSpan(dayIndex, 0, 0, 0)).Date;
            var lastDay = (firstDay + new TimeSpan(7, 0, 0, 0)).Date;

            return GetReportDatas(firstDay, lastDay);
        }

        public static object GetMonthlyData(DateTime date)
        {
            var firstDay = new DateTime(date.Year, date.Month, 1);
            var lastDay = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month))+new TimeSpan(1,0,0,0);
            return GetReportDatas(firstDay, lastDay);
        }

        /// <summary>
        /// 计算一天忙碌状态,计算前30天的
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static object GetBusyStatus(DateTime date)
        {
            return null;
            //using (var entity = new Models.DonataEntities())
            //{
            //    var start = date - new TimeSpan(30, 0, 0, 0);
            //    var end = date;

            //    var phoneOrders = (from po in entity.PhoneOrders
            //                       where po.Time >= start && po.Time <= end
            //                       select po).ToList();
            //    var dineOrders = (from dine in entity.DineOrders
            //                      where dine.Time >= start && dine.Time <= end
            //                      select dine).ToList();

            //    var groupPhone = from o in phoneOrders
            //                     group o by new { o.Time.Hour, o.Node } into g
            //                     orderby g.Key.Node, g.Key.Hour
            //                     select new
            //                     {
            //                         Node = g.Key.Node,
            //                         Total = (g.Sum(item => item.Time < TurnningPoint ? (item.Total * item.Discount / 10).Rounding() : item.Total)/30).Rounding(),
            //                         Hour=g.Key.Hour,
            //                         Count=g.Count()
            //                     };

            //    var groupDine = from o in dineOrders
            //                    group o by new { o.Time.Hour, o.Node } into g
            //                    orderby g.Key.Node, g.Key.Hour
            //                    select new
            //                    {
            //                        Node = g.Key.Node,
            //                        Total = (g.Sum(item => item.Time < TurnningPoint ? (item.Total * item.Discount / 10).Rounding() : item.Total)/30).Rounding(),
            //                        Hour=g.Key.Hour
            //                    };
            //    var nodesDine = (from o in dineOrders
            //                     select o.Node).Distinct();
            //    var nodesPhone = (from o in phoneOrders
            //                      select o.Node).Distinct();

            //    var nodes = nodesDine.Union(nodesPhone).Distinct();

            //    var Hours = (from d in groupDine
            //                 select d.Hour)
            //               .Union(
            //                from p in groupPhone
            //                select p.Hour)
            //                .Distinct().OrderBy(i => i);

            //    return new
            //    {
            //        Phone = groupPhone,
            //        Dine = groupDine,
            //        NodesList=nodes,
            //        Hours=Hours
            //    };
            //}
        }




    }
}