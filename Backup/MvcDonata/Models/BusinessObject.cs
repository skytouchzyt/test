#define mongodb

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

        public static List<string> Nodes
        {
            get
            {
                return new List<string> { "传媒店", "二外店", "管庄店", "青年汇店" };
            }
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
        public static string GetImageFileName(DishModel dish)
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

//        public static IEnumerable<IGrouping<string,DishModel>> GetGroupDishes()
//        {
//#if mongodb

//#else
//            using (var entity = new Models.DonataEntities())
//            {
//                var query = from d in entity.Dishes 
//                                    select d;
//                var r = from dish in query.ToList()
//                            orderby dish.ClassName,dish.Name
//                        select dish.Convert<Models.DishModel>();

//                var temp= from dish in r
//                       group dish by dish.ClassName into g
//                       select g;



//                return temp;
//            }
//#endif
//        }

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

        public static List<DishModel> GetDishes(string node,PhoneOrderType pot=PhoneOrderType.堂食)
        {


            using (var entity = new Models.DonataEntities())
            {
                var query = from d in entity.Dishes
                            select d;
                var r = from dish in query.ToList()
                        orderby dish.ClassName, dish.Name
                        select new DishModel
                        {
                            CanDiscounted = dish.CanDiscounted,
                            ClassName = dish.ClassName,
                            DefaultBar = dish.DefaultBar,
                            ID = dish.ID,
                            Name = GetNormalName(dish.Name),
                            PrintingName = GetPrintingName(dish.Name),
                            Price = dish.Price
                        };

                //获取最近一个月餐品出售数量
                var startDate = DateTime.Now - new TimeSpan(30, 0, 0, 0);
                var dic = new Dictionary<string, int>();
                if (pot == PhoneOrderType.堂食)
                {
                    var qc = from d in entity.OrderDetails
                             from o in entity.DineOrders
                             where d.OrderType == (int)DishOrderDetailType.堂食订单 && d.OrderID == o.ID
                             where o.Time>=startDate
                             group d by d.Name into g
                             select new
                             {
                                 Name = g.Key,
                                 Count = g.Sum(de=>de.Count)
                             };
                    var qq = from d in qc.ToList()
                         group d by GetDishRealName(d.Name) into g
                         select new
                             {
                                 Name = g.Key,
                                 Count = g.Sum(de=>de.Count)
                             };
                    qq.ToList().ForEach(item => dic.Add(item.Name, item.Count));
                }
                else
                {
                    var qc = from d in entity.OrderDetails
                             from o in entity.DineOrders
                             where d.OrderType == (int)DishOrderDetailType.电话订单 && d.OrderID == o.ID
                             where o.Time>=startDate
                             group d by d.Name into g
                             select new
                             {
                                 Name = g.Key,
                                 Count = g.Sum(de=>de.Count)
                             };
                    var qq = from d in qc.ToList()
                             group d by GetDishRealName(d.Name) into g
                             select new
                             {
                                 Name = g.Key,
                                 Count = g.Sum(de=>de.Count)
                             };

                    qq.ToList().ForEach(item => dic.Add(item.Name, item.Count));
                }

                var dishes = (from item in r
                              orderby item.PY descending
                              select item).ToList();
                dishes.ForEach(item =>
                    {
                        var real = GetDishRealName(item.Name);
                        if (dic.ContainsKey(real))
                            item.Count = dic[real];
                        item.CanSold = true;
                    }
                );

                //设置餐品是否售完的状态
                (from s in entity.SoldOut
                 where s.Node == node
                 select s.DishName)
                 .ToList().ForEach(item =>
                     {
                         var temp = from d in dishes
                                    where d.RealName == item
                                    select d;
                         if (temp.Count() > 0)
                             temp.First().CanSold = false;
                     }
                );


                var list= dishes.OrderByDescending(item => item.PY).ThenByDescending(item => item.Count).ToList();
                return list;
            }
        }

        public static void SetSoldOutDish(string node, string dishName, bool soldOut)
        {
            using (var entity = new DonataEntities())
            {
                var query = from s in entity.SoldOut
                            where s.Node == node && s.DishName == dishName
                            select s;
                if (soldOut==false&&query.Count()>0)
                {
                    var s = query.First();
                    entity.SoldOut.DeleteObject(s);
                    entity.SaveChanges();
                }
                else if (soldOut == true && query.Count() == 0)
                {
                    entity.SoldOut.AddObject(new SoldOut
                    {
                        DishName = dishName,
                        Node = node
                    }
                    );
                    entity.SaveChanges();
                }
            }
        }

        public static  IEnumerable<IGrouping<string,DishModel>> GetDishesOrderByPY()
        {
            var list = GetDishes("test",PhoneOrderType.外卖);
            var query = from dish in list
                        group dish by dish.Name.Substring(0,1).FirstPY() into g
                        orderby g.Key
                        select g;
            return query;
        }

        public static void InsertDish(DishModel dish)
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from d in entity.Dishes
                            where d.Name == dish.Name
                            select d;
                if (query.Count() > 0)
                    throw new Exception("已经添加过此餐品");
                entity.Dishes.AddObject(dish.Convert<Dishes>());
                entity.SaveChanges();
            }
        }
        public static void UpdateDish(DishModel dish)
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from d in entity.Dishes
                            where d.Name == dish.Name && d.ID != dish.ID
                            select d;
                if (query.Count() > 0)
                    throw new Exception("已经有此餐品");
                query = from d in entity.Dishes
                        where d.ID == dish.ID
                        select d;
                var di = query.First();
                di.ClassName = dish.ClassName;
                di.DefaultBar = dish.DefaultBar;
                di.Name = dish.Name;
                di.Price = dish.Price;
                di.CanDiscounted = dish.CanDiscounted;

                entity.SaveChanges();
            }
        }

        public static void UpdateAllDishes(List<Models.DishModel> dishes)
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from d in entity.Dishes
                            select d;
                query.ToList().ForEach(i => entity.Dishes.DeleteObject(i));
                entity.SaveChanges();

                dishes.ForEach(
                    dish =>
                    {
                        if (dish.Name != dish.PrintingName)
                        {
                            dish.Name = dish.Name + "," + dish.PrintingName;
                        }
                        entity.Dishes.AddObject(dish.Convert<Dishes>());
                    }
                );
                entity.SaveChanges();
            }
        }

        public static DishModel GetDishByID(int id)
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from d in entity.Dishes
                            where d.ID == id
                            select d;
                if (query.Count() == 0)
                    throw new Exception(string.Format("没有找到指定ID为[{0}]的餐品.", id));
                else
                    return query.First().Convert<DishModel>();
            }
        }

        public static List<string> GetBars()
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from d in entity.Dishes
                            select d.DefaultBar;
                var list = new List<string>();

                query.Distinct().ToList().ForEach(i => list.AddRange(i.Split(new char[] { ',' })));
                return list.Distinct().ToList();
            }
        }

        public static void DeleteDish(int id)
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from d in entity.Dishes
                            where d.ID == id
                            select d;
                entity.Dishes.DeleteObject(query.First());
                entity.SaveChanges();
            }
        }

        #endregion

        #region 顾客操作
        /// <summary>
        /// 第一页为1
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="address"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static List<CustomerModel> GetCustomers(out int pageCount,string phone="", string address="",int page=1,int pageSize=5)
        {
            page--;
            if (page < 0) page = 0;
            using (var entity = new DonataEntities())
            {
                if (!string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(address))
                {
                    //根据电话和地址查询
                    var query1 = from c in entity.Customers
                                 from a in entity.Addresses
                                 where c.ID == a.CustomerID
                                 where a.Address.Contains(address)
                                 where c.Phone.Contains(phone)
                                 orderby c.ID
                                 select new CustomerModel
                                 {
                                     Count = c.Count,
                                     CreatedTime = c.CreatedTime,
                                     ID = c.ID,
                                     LastOrderTime = c.LastOrderTime,
                                     Name = c.Name,
                                     Phone = c.Phone,
                                     Sex = (Sex)c.Sex,
                                     Total = c.Total
                                 };




                    pageCount = query1.Distinct().Count() / pageSize;

                    var result = query1.Distinct().OrderBy(item => item.ID).Skip(page * pageSize).Take(pageSize).ToList();
                    foreach (var c in result)
                    {
                        c.Addresses = GetAddressesByCustomerID(c.ID);
                    }
                    return result;
                }
                else if (!string.IsNullOrEmpty(phone))
                {
                    //根据电话查询
                    var query1 = from c in entity.Customers
                                 from a in entity.Addresses
                                 where c.ID == a.CustomerID
                                 where c.Phone.Contains(phone)
                                 orderby c.ID
                                 select new CustomerModel
                                 {
                                     Count = c.Count,
                                     CreatedTime = c.CreatedTime,
                                     ID = c.ID,
                                     LastOrderTime = c.LastOrderTime,
                                     Name = c.Name,
                                     Phone = c.Phone,
                                     Sex = (Sex)c.Sex,
                                     Total = c.Total
                                 };




                    pageCount = query1.Distinct().Count() / pageSize;

                    var result = query1.Distinct().OrderBy(item => item.ID).Skip(page * pageSize).Take(pageSize).ToList();
                    foreach (var c in result)
                    {
                        c.Addresses = GetAddressesByCustomerID(c.ID);
                    }
                    return result;
                }
                else if (!string.IsNullOrEmpty(address))
                {
                    //根据地址查询
                    var query1 = from c in entity.Customers
                                 from a in entity.Addresses
                                 where c.ID == a.CustomerID
                                 where a.Address.Contains(address)
                                 orderby c.ID
                                 select new CustomerModel
                                 {
                                     Count = c.Count,
                                     CreatedTime = c.CreatedTime,
                                     ID = c.ID,
                                     LastOrderTime = c.LastOrderTime,
                                     Name = c.Name,
                                     Phone = c.Phone,
                                     Sex = (Sex)c.Sex,
                                     Total = c.Total
                                 };




                    pageCount = query1.Distinct().Count() / pageSize;

                    var result = query1.Distinct().OrderBy(item => item.ID).Skip(page * pageSize).Take(pageSize).ToList();
                    foreach (var c in result)
                    {
                        c.Addresses = GetAddressesByCustomerID(c.ID);
                    }
                    return result;
                }
                else
                {
                    //查询所有的
                    //根据电话和地址查询
                    var query1 = from c in entity.Customers
                                        orderby c.ID
                                 select new CustomerModel
                                 {
                                     Count = c.Count,
                                     CreatedTime = c.CreatedTime,
                                     ID = c.ID,
                                     LastOrderTime = c.LastOrderTime,
                                     Name = c.Name,
                                     Phone = c.Phone,
                                     Sex = (Sex)c.Sex,
                                     Total = c.Total
                                 };




                    pageCount = query1.Distinct().Count() / pageSize;

                    var result = query1.Distinct().OrderBy(item => item.ID).Skip(page * pageSize).Take(pageSize).ToList();
                    foreach (var c in result)
                    {
                        c.Addresses = GetAddressesByCustomerID(c.ID);
                    }
                    return result;
                }
            }
        }

        public static CustomerModel GetCustomer(int CustomerID,int? AddressID=null)
        {
            using (var entity = new DonataEntities())
            {
                var query = from c in entity.Customers
                            where c.ID==CustomerID
                            select new CustomerModel
                            {
                                Count = c.Count,
                                CreatedTime = c.CreatedTime,
                                ID = c.ID,
                                LastOrderTime = c.LastOrderTime,
                                Name = c.Name,
                                Phone = c.Phone,
                                Sex = (Sex)c.Sex,
                                Total = c.Total,
         
                            };

                var cm = query.First();
                if (!AddressID.HasValue)
                    cm.Addresses = GetAddressesByCustomerID(cm.ID);
                else
                {
                    var querya = from a in entity.Addresses
                                 where a.ID == AddressID
                                 select new AddressModel
                                 {
                                     Address = a.Address,
                                     City = a.City,
                                     ID = a.ID,
                                     Count = a.Count,
                                     CustomerID = a.CustomerID,
                                     LastNode = a.LastNode,
                                     LastUsedTime = a.LastUsedTime,
                                 };
                    cm.Addresses = querya.ToList();
                }

                cm.Addresses.ForEach(item => item.py = item.Address.FirstPY());

                return cm;
            }
        }

        public static CustomerModel GetCustomer(DonataEntities entity, string phone)
        {
            var queryc = from c in entity.Customers
                         where c.Phone.Contains(phone)
                         select c;
            var r = from c in queryc.ToList()
                    where c.Phone.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains(phone)
                    select c;

            if (r.Count() == 0)
                throw new Exception("没有找到指定的顾客.");

            var result = new CustomerModel();
            var c1 = r.First();
            result.ID = c1.ID;
            result.ActivedDegree = c1.ActivedDegree;
            result.Addresses = GetAddressesByCustomerID(c1.ID);
            result.Count = c1.Count;
            result.CreatedTime = c1.CreatedTime;
            result.LastOrderTime = c1.LastOrderTime;
            result.Name = c1.Name;
            result.Phone = c1.Phone;
            result.Sex = (Sex)c1.Sex;
            result.Total = c1.Total;

            return result;
        }

        public static CustomerModel GetCustomer(string phone)
        {
            using (var entity = new DonataEntities())
            {
                var queryc = from c in entity.Customers
                             where c.Phone.Contains(phone)
                             select c;
                var r = from c in queryc.ToList()
                        where c.Phone.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains(phone)
                        select c;

                if (r.Count() == 0)
                    return new CustomerModel
                    {
                         Phone=phone
                    };

                var result = new CustomerModel();
                var c1 = r.First();
                result.ID = c1.ID;
                result.ActivedDegree = c1.ActivedDegree;
                result.Addresses = GetAddressesByCustomerID(c1.ID);
                result.Count = c1.Count;
                result.CreatedTime = c1.CreatedTime;
                result.LastOrderTime = c1.LastOrderTime;
                result.Name = c1.Name;
                result.Phone = c1.Phone;
                result.Sex = (Sex)c1.Sex;
                result.Total = c1.Total;

                return result;
            }
        }

        
        public static List<AddressModel> GetAddressesByCustomerID(int customerID)
        {
            using (var entity = new DonataEntities())
            {
                var query = from a in entity.Addresses
                            where a.CustomerID == customerID
                            orderby a.LastUsedTime descending
                            select new AddressModel
                            {
                                Address = a.Address,
                                LastNode = a.LastNode,
                                City = a.City,
                                Count = a.Count,
                                CustomerID = a.CustomerID,
                                ID = a.ID,
                            };

                var list= query.ToList();
                list.ForEach(item => item.py = item.Address.FirstPY());
                return list;
            }
        }
        public static void InsertAddresses(int CustomerID,IEnumerable<AddressModel> list)
        {
            
        }

        /// <summary>
        /// 更新地址
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <param name="list"></param>
        /// <param name="deleteOldAddress"></param>
        public static void UpdateAddresses(int CustomerID, List<AddressModel> list,bool overide=false)
        {
            using (var entity = new DonataEntities())
            {
                foreach (var addr in list)
                {
                    var query = from a in entity.Addresses
                                where CustomerID == a.CustomerID
                                where a.Address == addr.Address && a.City==addr.City
                                select a;
                    if (query.Count() == 0) //如果没有此地址加入
                    {
                        var address = new Addresses
                        {
                            Address = addr.Address,
                            CustomerID = CustomerID,
                            City = addr.City,
                            Count = 0,
                            LastNode = addr.LastNode,
                            LastUsedTime = addr.LastUsedTime
                        };
                        entity.Addresses.AddObject(address);
                        entity.SaveChanges();
                        addr.ID = address.ID;
                    }
                    else
                    {

                        var source = query.First();
                        addr.ID = source.ID;
                    }

                }
                

                if (overide)
                {
                    var already = from a in list
                                  select a.Address;
                    var del = from a in (from a in entity.Addresses
                              where a.CustomerID == CustomerID select a).ToList()
                              where already.ToList().IndexOf(a.Address) < 0
                              select a;
                    foreach (var a in del)
                    {
                        entity.Addresses.DeleteObject(a);
                    }
                    entity.SaveChanges();
                }
            }
        }
      

        /// <summary>
        /// 判断用户是否存在
        /// </summary>
        /// <param name="cm"></param>
        /// <returns></returns>
        public static bool IsCustomerExistsByPhone(dynamic cm)
        {
            var phone = (string)cm.Phone;
            using (var entity = new DonataEntities())
            {
                var queryc = from c in entity.Customers
                             where c.Phone.Contains(phone)
                             select c;
                var r = from c in queryc.ToList()
                        where c.Phone.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains(phone)
                        select c;

                return r.Count() > 0;
            }
        }
        public static CustomerModel InsertCustomer(dynamic cm)
        {
            using (var entity = new DonataEntities())
            {
                if(IsCustomerExistsByPhone(cm))
                    throw new Exception(string.Format("此用户[{0}]已经加入.",cm.Phone));



                var cust = new Customers
                {
                    Count = 0,
                    Phone = cm.Phone,
                    Total = 0,
                    Sex = (int)cm.Sex,
                    Name = cm.Name,
                    LastOrderTime = null,
                    CreatedTime = DateTime.Now
                };
                entity.Customers.AddObject(cust);
                entity.SaveChanges();


                foreach (var a in cm.Addresses)
                {
                    var addr = new Addresses
                    {
                        Address = a.Address,
                        CustomerID = cust.ID,
                        City = a.City,
                        Count = 0,
                        LastNode = a.LastNode,

                    };

                    entity.Addresses.AddObject(addr);
                }
                entity.SaveChanges();

                cm.ID = cust.ID;

                return cm;
            }
        }

        public static void UpdateCustomer(CustomerModel customer,bool overide=false)
        {

            using (var entity = new DonataEntities())
            {
                var query = from c in entity.Customers
                            where c.ID == customer.ID
                            select c;
                if(query.Count()==0)
                    throw new Exception(string.Format("指定的顾客[{0}]不存在.", customer.ID));
                var cust = query.First();
                
                cust.Name = customer.Name=="请找机会询问"?null:customer.Name;
                cust.Phone = customer.Phone;
                cust.Sex = (int)customer.Sex;

                entity.SaveChanges();

                UpdateAddresses(customer.ID, customer.Addresses,overide);                
                
            }
        }
    


        #endregion

        #region 电话订单操作

        public static void NewPhoneCall(int number,string phone, DateTime time)
        {
            using (var entity = new DonataEntities())
            {
                var call = new CallRecord
                {
                    Phone = phone,
                    Time = time,
                    BarNumber=number,
                    Received=false
                };
                entity.CallRecord.AddObject(call);
                entity.SaveChanges();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string GetNewPhoneCall(int number)
        {
            using (var entity = new DonataEntities())
            {
                var query = from c in entity.CallRecord
                            where EntityFunctions.DiffSeconds(c.Time, DateTime.Now) < 30
                            where c.BarNumber==number
                            where c.Received==false
                            select c;

                if (query.Count() > 0)
                {
                    var c = query.First();
                    c.Received = true;
                    entity.SaveChanges();
                    return c.Phone;
                }
                else
                    return "";
            }
        }

        public static bool IsPhoneOrderExists(PhoneOrderModel po)
        {
            //根据条形码判断此电话订单是否存在
            using (var entity = new DonataEntities())
            {
                var query = from p in entity.PhoneOrders
                            where p.BarCode == po.BarCode
                            select p;
                return query.Count() > 0;
            }
        }

        public static void InsertPhoneOrder(List<PhoneOrderModel> list)
        {

        }
        public static List<PhoneOrderModel> GetNewPhoneOrders(string node)
        {
            try
            {
                using (var entity = new DonataEntities())
                {
                    var query = from order in entity.PhoneOrders
                                orderby order.Time descending
                                //where EntityFunctions.DiffHours(DateTime.Now, order.Time) <24 
                                where order.Node==node
                                where order.Received==false
                                where order.Completed //只获取写入完整的外送订单
                                select new PhoneOrderModel
                                {
                                    BarCode = order.BarCode,
                                    CustomerID = order.CustomerID,
                                    AddressID = order.AddressID,
                                    CustomerCount = order.CustomerCount,
                                    Discount = order.Discount,
                                    EmployeeID = order.EmployeeID,
                                    ID = order.ID,
                                    Node = order.Node,
                                    PhoneOrderType = (Models.PhoneOrderType)order.PhoneOrderType,
                                    Received = order.Received,
                                    Remark = order.Remark,
                                    Scheduled = order.Scheduled,
                                    Time = EntityFunctions.AddHours(order.Time, 8).Value,
                                    Total = order.Total
                                };
                    var list = (from o in query.ToList()
                                where (DateTime.Now - o.Time).TotalHours < 24 //搜索24小时内没有收到的单子
                                select o).ToList();
                    list.ForEach(item =>
                        {
                            item.Details = (List<DishOrderDetailModel>)GetPhoneOrderDetails(item.ID);
                            item.Customer = GetCustomer(item.CustomerID, item.AddressID);

                        }

                       );
                    return list;
                }
            }
            catch (Exception e)
            {
                return new List<PhoneOrderModel>();
            }
        }
        public static object SignPhoneOrder(string barcode)
        {
            try
            {
                using (var entity = new DonataEntities())
                {
                    var query = from o in entity.PhoneOrders
                                where o.BarCode == barcode
                                select o;
                    if (query.Count() == 0)
                        throw new Exception(string.Format("没有找到编号为:[{0}]的订单.", barcode));
                    var po = query.First();
                    po.Received = true;
                    entity.SaveChanges();
                    return new iJsonResult("success");
                }
            }
            catch (Exception e)
            {
                return new iJsonResult(null, e.Message);
            }
        }
        public static void InsertPhoneOrder(PhoneOrderModel po)
        {


            if (!IsCustomerExistsByPhone(po.Customer))
            {
                InsertCustomer(po.Customer);
            }
            po.Customer.ID = GetCustomer(po.Customer.Phone).ID;


            using (var entity = new DonataEntities())
            {


                if (string.IsNullOrEmpty(po.BarCode))
                {
                    //如果是新订单就计算出条码
                    var count = (from p in entity.PhoneOrders
                                 select p.ID).Count();
                    po.BarCode = string.Format("P{0:d8}", count + 10000);

                }

                UpdateCustomer(po.Customer);

                var query = from c in entity.Customers
                            where c.ID == po.Customer.ID
                            select c;
                var cust = query.First();

                var phoneOrder = new Models.PhoneOrders
                {
                    CustomerCount = po.CustomerCount,
                    Discount = po.Discount,
                    Node = po.Node,
                    Received = po.Received,
                    PhoneOrderType = (int)po.PhoneOrderType,
                    Remark = po.Remark,
                    Scheduled = po.Scheduled,
                    Time = po.Time,
                    Total = po.Total,
                     CustomerID=cust.ID,
                    AddressID=po.Customer.Addresses[0].ID,
                    BarCode=po.BarCode
                };

                //更新最后下单时间,取最大的时间
                if (cust.LastOrderTime==null||cust.LastOrderTime.Value < po.Time)
                {
                    cust.LastOrderTime = po.Time;
                }
                // 更新订单次数和总金额
                cust.Count++;
                cust.Total += po.Total;

                //更新创建时间,取最小的时间为创建时间
                if (cust.CreatedTime > po.Time)
                {
                    cust.CreatedTime = po.Time;
                }

                entity.PhoneOrders.AddObject(phoneOrder);
                entity.SaveChanges();

                //插入订单明细
                foreach (var detail in po.Details)
                {
                    entity.OrderDetails.AddObject(
                        new OrderDetails
                        {
                            Count = detail.Amount,
                            Name = detail.Name,
                            OrderID = phoneOrder.ID,
                            OrderType = (int)DishOrderDetailType.电话订单,
                            Price = detail.Price,
                            Remark = detail.Remark
                        }
                        );

                }
                entity.SaveChanges();

                phoneOrder.Completed = true; //指示订单数据完全写入

                entity.SaveChanges();

                UpdateAddresses(po.Customer.ID, po.Customer.Addresses);

                //更新地址使用信息
                var targetAddr = po.Customer.Addresses[0].Address;
                var q = from a in entity.Addresses
                        where a.CustomerID == po.Customer.ID
                        where a.Address == targetAddr
                        select a;
                var addr = q.First();
                addr.Count++;
                addr.LastNode = po.Node;
                addr.LastUsedTime = po.Time;

                entity.SaveChanges();


                //如果不是手机号就不需要发送短信
                if (po.Customer.Phone.Length != 11)
                    return;

                //检测是否有退单，如果有退单就不发送短信
                var disorderCount=(from d in po.Details
                                   where d.Amount<0
                                       select d
                                       ).Count();
                if (disorderCount > 0)
                    return;

                //早上10:00到晚上11:00才发送短信，防止骚扰客户
                var start = new TimeSpan(10, 0, 0);
                var end = new TimeSpan(23, 0, 0);
                var now=DateTime.Now.TimeOfDay;
                if (now <= start || now >= end)
                    return;



                using (var client = new WebService.LinkWSSoapClient())
                {
                    var sms = "亲，您订的餐品是：{0},共计：{1}元。我们将为您尽快送达。谢谢您的惠顾,欢迎再次光临！现在微信可以点外卖了，并且免外送费，请用微信查找公众账号“多纳达比萨”。(勿回)";

                    var details = new List<string>();
                    foreach (var d in po.Details)
                    {
                        if(string.IsNullOrEmpty(d.Remark))
                            details.Add(string.Format("{0}({1}份)", d.Name, d.Amount));
                        else
                            details.Add(string.Format("{0}({1})({2}份)", d.Name, d.Remark,d.Amount));
                    }

                    sms = string.Format(sms, details.JoinByChar(','), po.Total);

                    client.Send("tclkj02192", "zytzly97", po.Customer.Phone, sms, "", "");
                }

                


            }

        }
        #endregion

        #region 堂食订单操作

        public static string CreateNewBarCode(string node,string server)
        {

                //首先从本地数据库获取BarCode
            var last = GetLastDineOrderBarCode(node);
            if (string.IsNullOrEmpty(last)) //如果为空就从服务器上获取
            {
                last = ("http://" + server + "/DineOrder/GetLastBarCode").SendGetRequest(new { node = node }, 20);
            }
            if (string.IsNullOrEmpty(last))
            {
                //如果还是为空就自己创建
                return string.Format("D{0}{1:d7}", node.FirstPY().ToUpper(), DateTime.Now.Year * 10 + 1000000 + 365 * 200 + 1);
            }
            var pre = node.FirstPY().ToUpper();
            var id = int.Parse(last.Substring(pre.Length+1)) + 1;

            return string.Format("D{0}{1:d7}",pre,id);         
            
        }

        public static string GetLastDineOrderBarCode(string node)
        {
            using (var entity = new DonataEntities())
            {
                var query = from d in entity.DineOrders
                            where d.Node==node
                            orderby d.Time descending
                            select d.BarCode;
                return query.FirstOrDefault();
            }
        }

        public static void InsertDineOrder(Models.DineOrderModel order)
        {
            using (var entity = new DonataEntities())
            {
                //首先检测此单是否已经写入,如果写入就直接返回
                var query = from o in entity.DineOrders
                            where o.BarCode == order.BarCode
                            where Math.Abs(EntityFunctions.DiffDays(o.Time,order.Time).Value)<1
                            select o;
                if (query.Count() > 0)
                    return;
                var dineorder = new Models.DineOrders
                {
                    BarCode = order.BarCode,
                    CustomerCount = order.CustomerCount,
                    Discount = order.Discount,
                    EmployeeID = order.EmployeeID,
                    Node = order.Node,
                    TableNumber = order.TableNumber,
                    Time = order.Time.ToLocalTime(),
                    Total = order.Total
                };
                entity.DineOrders.AddObject(dineorder);
                entity.SaveChanges();
                //再写入订单明细
                foreach (var d in order.Details)
                {
                    var detail = new Models.OrderDetails
                    {
                        Count = d.Amount,
                        Index = d.Index,
                        ParentIndex = d.ParentIndex,
                        Name = d.Name,
                        OrderType = (int)DishOrderDetailType.堂食订单,
                        OrderID = dineorder.ID,
                        Price = d.Price,
                        Remark = d.Remark
                    };
                    entity.OrderDetails.AddObject(detail);
                }
                entity.SaveChanges();
            }

            //写入到MongoDB数据库
            try
            {
                var dine = new DineOrder();
                dine.BarCode = order.BarCode;
                dine.Total = order.Total;
                dine.Discount = order.Discount;
                dine.Node = order.Node;
                dine.Remark = order.Remark;
                dine.Time = order.Time.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                dine.TableNumber = order.TableNumber;
                dine.CustomerCount = order.CustomerCount;

                foreach (var d in order.Details)
                {
                    dine.Details.Add(new DishOrderDetail
                    {
                        Amount = d.Amount,
                        Remark = d.Remark,
                        DishName = d.Name,
                        Price = d.Price
                    }
                    );
                }

                var db = "180.186.21.156".MongoDB("donata");
                var collection = db.GetCollection<DineOrder>("DineOrders");

                

                //首先查询是否已经写入到数据库
                //如果是的话就不需要重复写入
                var query = from d in collection.AsQueryable()
                            where d.Time == dine.Time
                            where d.BarCode == dine.BarCode
                            select d;

                if(query.Count()==0)
                    collection.Insert(dine);

                

              
            }
            catch
            {

            }
        }
        #endregion

        /// <summary>
        /// 如果此分店目前有订单就会出现在列表中,否则就不会
        /// 要区别于GetNodes
        /// </summary>
        /// <param name="selectedDate"></param>
        /// <returns></returns>
        public static List<string> GetNodesByOrders(DateTime selectedDate)
        {
            using (var entity = new DonataEntities())
            {
                var queryP = from p in entity.PhoneOrders
                            where EntityFunctions.DiffDays(p.Time,selectedDate)==0
                            group p by p.Node into g
                            select g.Key;
                var queryD = from d in entity.DineOrders
                             where EntityFunctions.DiffDays(d.Time, selectedDate) == 0
                             group d by d.Node into g
                             select g.Key;
                return queryD.Union(queryP).Distinct().ToList();
            }
        }


        public static Tuple<int,List<PhoneOrderModel>,int> GetPhoneOrders(
            bool paged,
            int pageSize,
            int pageIndex,
            string node,
            DateTime selectedDate
            )
        {
            int pageCount = 0;
            int total = 0;
            using (var entity = new DonataEntities())
            {
                var query = from order in entity.PhoneOrders
                            orderby order.Time descending
                            where EntityFunctions.DiffDays(selectedDate, order.Time) == 0
                            select new PhoneOrderModel
                            {
                                BarCode = order.BarCode,
                               CustomerID=order.CustomerID,
                               AddressID=order.AddressID,
                                CustomerCount = order.CustomerCount,
                                Discount = order.Discount,
                                EmployeeID = order.EmployeeID,
                                ID = order.ID,
                                Node = order.Node,
                                PhoneOrderType = (Models.PhoneOrderType)order.PhoneOrderType,
                                Received = order.Received,
                                Remark = order.Remark,
                                Scheduled = order.Scheduled,
                                Time = order.Time,
                                Total = order.Total
                            };

                if (node != "全部")
                {
                    query = query.Where(i => i.Node == node);
                }

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
                        item.Total = item.Time < TurnningPoint ? (item.Total * item.Discount / 10).Rounding() : item.Total;
                        item.Customer = GetCustomer(item.CustomerID, item.AddressID);
                    }
                );


                return new Tuple<int, List<PhoneOrderModel>,int>(pageCount, list,total);
            }
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


            var db = "180.186.21.156".MongoDB("donata");
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
                        nodes = GetNodesByOrders(selectedDate),
                        pageIndex = pageIndex
                    };

            //下面的代码使用sql server数据库
            //using (var entity = new DonataEntities())
            //{
            //    var query = (from order in entity.DineOrders
            //                 orderby order.Time descending
            //                 where EntityFunctions.DiffDays(selectedDate, order.Time)==0
            //                 select new DineOrderModel
            //                 {
            //                     BarCode = order.BarCode,
            //                     CustomerCount = order.CustomerCount,
            //                     Discount = order.Discount,
            //                     EmployeeID = order.EmployeeID,
            //                     ID = order.ID,
            //                     Node = order.Node,
            //                     TableNumber = order.TableNumber,
            //                     Time = order.Time,
            //                     Total =order.Total,
            //                     Remark=order.Remark
            //                 });


            //    if (node != "全部")
            //    {
            //        query = query.Where(i => i.Node == node);
            //    }

            //    if (paged)
            //    {
            //        if (pageSize != 0)
            //        {
            //            var total = query.Count();
            //            pageCount = (int)Math.Ceiling(((float)total / pageSize));                   
            //        }
            //        pageIndex = Math.Min(pageCount-1, pageIndex);
            //        pageIndex = Math.Max(0, pageIndex);
            //        query = query.Skip(pageSize * pageIndex).Take(pageSize);
            //    }

            //    var list = query.ToList();

            //    //计算实际总额 总额*打折 然后四舍五入 
            //    //TurnningPoint以前的订单总计需要计算得出,以后的总计直接读取数据库
            //    list.ForEach(item => item.Total = item.Time<TurnningPoint?(item.Total * item.Discount / 10).Rounding():item.Total);


            //    return new
            //        {
            //            pageCount = pageCount,
            //            datas = list,
            //            nodes=GetNodesByOrders(selectedDate),
            //            pageIndex=pageIndex
            //        };
            //}
        }
        public static List<DishOrderDetailModel> GetPhoneOrderDetails(int orderID)
        {
            using (var entity = new DonataEntities())
            {
                var query=from d in entity.OrderDetails
                          where d.OrderType==(int)Models.DishOrderDetailType.电话订单
                          where d.OrderID==orderID
                          orderby d.ID
                          select new Models.DishOrderDetailModel
                          {
                              Amount = d.Count,
                              ID = d.ID,
                              Index = d.Index,
                              Name = d.Name,
                              OrderID = d.OrderID,
                              OrderType = d.OrderType,
                              ParentIndex = d.ParentIndex,
                              Price = d.Price,
                              Remark = d.Remark
                          };

                return query.ToList();
            }
        }
        public  static object GetDineOrderDetails(int orderID)
        {
            using (var entity = new DonataEntities())
            {
                var query = from d in entity.OrderDetails
                            where d.OrderType == (int)Models.DishOrderDetailType.堂食订单
                            where d.OrderID == orderID
                            orderby d.ID
                            select new Models.DishOrderDetailModel
                            {
                                Amount = d.Count,
                                ID = d.ID,
                                Index = d.Index,
                                Name = d.Name,
                                OrderID = d.OrderID,
                                OrderType = d.OrderType,
                                ParentIndex = d.ParentIndex,
                                Price = d.Price,
                                Remark = d.Remark
                            };
                return new
                {
                    datas=query.ToList()
                };
            }
        }

        /// <summary>
        /// 根据表DNS中的记录返回分店列表
        /// 但是如果分店有2天没有更新就不会显示
        /// </summary>
        /// <returns></returns>
        public static List<Models.DNSModel> GetNodes()
        {
            using (var m = new Models.DonataEntities())
            {
                var query = from d in m.DNS
                            where EntityFunctions.DiffDays(d.UpdatedTime, DateTime.Now) < 2
                            orderby d.Node
                            select d;
                var r = from d in query.ToList()
                        select new DNSModel
                        {
                            IP = d.IP,
                            Node = d.Node,
                            Port = d.Port,
                            UpdatedTime = d.UpdatedTime,
                            TimeSpan = (int)(DateTime.Now - d.UpdatedTime).TotalMinutes,
                             ApplicationVersion=d.ApplicationVersion
                        };
                return r.ToList();
            }
        }

        public static IEnumerable<object> GetSoldoutDishes()
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from s in entity.SoldOut
                            select new
                            {
                                Node = s.Node,
                                DishName = s.DishName
                            };
                return query.ToList().Cast<object>();
            }
        }

        public static object GetReportDatas(DateTime start, DateTime end)
        {
            using (var entity = new Models.DonataEntities())
            {
                

                var phoneOrders = (from po in entity.PhoneOrders
                                   where po.Time >= start && po.Time <= end
                                   select po).ToList();
                var dineOrders = (from dine in entity.DineOrders
                                  where dine.Time >= start && dine.Time <= end
                                  select dine).ToList();

                var groupPhone = from o in phoneOrders
                                 group o by new { o.Time.Date, o.Node } into g
                                 orderby g.Key.Node, g.Key.Date
                                 select new
                                 {
                                     Node = g.Key.Node,
                                     IndexOfWeek = (int)g.Key.Date.DayOfWeek,
                                     Total = g.Sum(item => item.Time < TurnningPoint ? (item.Total * item.Discount / 10).Rounding() : item.Total),
                                     Date = g.Key.Date.Day
                                 };

                var groupDine = from o in dineOrders
                                group o by new { o.Time.Date, o.Node } into g
                                orderby g.Key.Node, g.Key.Date
                                select new
                                {
                                    Node = g.Key.Node,
                                    IndexOfWeek = (int)g.Key.Date.DayOfWeek,
                                    Total = g.Sum(item => item.Time < TurnningPoint ? (item.Total * item.Discount / 10).Rounding() : item.Total),
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
            using (var entity = new Models.DonataEntities())
            {
                var start = date - new TimeSpan(30, 0, 0, 0);
                var end = date;

                var phoneOrders = (from po in entity.PhoneOrders
                                   where po.Time >= start && po.Time <= end
                                   select po).ToList();
                var dineOrders = (from dine in entity.DineOrders
                                  where dine.Time >= start && dine.Time <= end
                                  select dine).ToList();

                var groupPhone = from o in phoneOrders
                                 group o by new { o.Time.Hour, o.Node } into g
                                 orderby g.Key.Node, g.Key.Hour
                                 select new
                                 {
                                     Node = g.Key.Node,
                                     Total = (g.Sum(item => item.Time < TurnningPoint ? (item.Total * item.Discount / 10).Rounding() : item.Total)/30).Rounding(),
                                     Hour=g.Key.Hour,
                                     Count=g.Count()
                                 };

                var groupDine = from o in dineOrders
                                group o by new { o.Time.Hour, o.Node } into g
                                orderby g.Key.Node, g.Key.Hour
                                select new
                                {
                                    Node = g.Key.Node,
                                    Total = (g.Sum(item => item.Time < TurnningPoint ? (item.Total * item.Discount / 10).Rounding() : item.Total)/30).Rounding(),
                                    Hour=g.Key.Hour
                                };
                var nodesDine = (from o in dineOrders
                                 select o.Node).Distinct();
                var nodesPhone = (from o in phoneOrders
                                  select o.Node).Distinct();

                var nodes = nodesDine.Union(nodesPhone).Distinct();

                var Hours = (from d in groupDine
                             select d.Hour)
                           .Union(
                            from p in groupPhone
                            select p.Hour)
                            .Distinct().OrderBy(i => i);

                return new
                {
                    Phone = groupPhone,
                    Dine = groupDine,
                    NodesList=nodes,
                    Hours=Hours
                };
            }
        }

    }
}