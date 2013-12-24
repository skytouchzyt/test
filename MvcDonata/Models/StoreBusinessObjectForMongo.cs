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
using System.Data.Objects;

namespace MvcDonata.Models
{
    public class StoreBusinessObjectForMongo
    {
        public static void InsertOrder(string employee, string node, string storeType, string remark, DateTime time, IEnumerable<StoreDetailModal> details)
        {
            time = time.Date + DateTime.Now.TimeOfDay; //前台提交过来的日期没有时间部分
            //这里加上当前的时间,防止如果当天有盘点,新加入的订单时间却比盘点时间早,而不会被统计

            var updatedAccessTime = true;
            if (storeType == "进货单" && node != "总库房") //不是总库房的进货单就不需要更新商品信息
                updatedAccessTime = false;

            using (var entity = new DonataEntities())
            {
                var store = new Store
                {
                    Employee = employee,
                    Node = node,
                    StoreType = storeType,
                    Remark = remark,
                    Time = time,
                    Total = details.Sum(item => item.Amount * item.Price).Rounding()
                };
                entity.Store.AddObject(store);
                entity.SaveChanges();

                foreach (var detail in details)
                {
                    if (detail.ProductID == 0)
                    {
                        detail.ProductID = AddProduct(detail, updatedAccessTime);
                    }
                    entity.StoreDetails.AddObject(new StoreDetails
                    {
                        Amount = detail.Amount,
                        ProductID = detail.ProductID,
                        Price = detail.Price,
                        StoreID = store.ID
                    });
                }
                entity.SaveChanges();



                //更新商品信息
                UpdateProducts(details, updatedAccessTime);


            }
        }
        /// <summary>
        /// 获取最近使用过的商品,只获取食材和消耗品
        /// </summary>
        public static List<StoreDetailModal> GetRecentProducts()
        {
            using (var entity = new DonataEntities())
            {
                var query = from p in entity.Products
                            where p.MaterialType == "食材" || p.MaterialType == "消耗品"
                            where p.Name != "燕子牌高糖即发干酵母"
                            where EntityFunctions.DiffDays(DateTime.Now, p.LastAccess) <= 8 //半个月内用到的货物
                            select new StoreDetailModal
                            {
                                ProductID = p.ID,
                                MaterialType = p.MaterialType,
                                Unit = p.Unit,
                                Standards = p.Standards,
                                Provider = p.Provider,
                                Remark = p.Remark,
                                Name = p.Name,
                                Price = p.Price
                            };
                var throughput = (from t in GetStock(new List<int>(), true)
                                  where t.Amount > 0
                                  select t.ProductID).ToList();
                var result = from r in query.ToList()
                             from t in throughput
                             where r.ProductID == t
                             orderby r.MaterialType, r.PY
                             select r;
                return result.ToList();

            }
        }


        public static int AddProduct(ProductModal product, bool updatedAccessTime = true)
        {
            using (var entity = new DonataEntities())
            {
                var p = new Products
                {
                    Name = product.Name,
                    LastAccess = updatedAccessTime ? DateTime.Now : DateTime.Now - new TimeSpan(30, 0, 0, 0),
                    Standards = product.Standards,
                    Remark = product.Remark,
                    Provider = product.Provider,
                    Price = product.Price,
                    Unit = product.Unit,
                    MaterialType = product.MaterialType
                };
                entity.Products.AddObject(p);
                entity.SaveChanges();

                return p.ID;

            }
        }

        /// <summary>
        /// 更新商品信息或者添加新的商品
        /// </summary>
        /// <param name="products"></param>
        public static void UpdateProducts(IEnumerable<ProductModal> products, bool updatedProduct = true)
        {
            using (var entity = new DonataEntities())
            {
                foreach (var product in products)
                {
                    var query = from p in entity.Products
                                where p.ID == product.ProductID
                                select p;
                    if (query.Count() == 0) //如果没有就加入新的商品
                    {
                        entity.Products.AddObject(
                            new Products
                            {
                                Name = product.Name,
                                LastAccess = updatedProduct ? DateTime.Now : DateTime.Now - new TimeSpan(30, 0, 0, 0),
                                Standards = product.Standards,
                                Remark = product.Remark,
                                Provider = product.Provider,
                                Price = product.Price,
                                Unit = product.Unit,
                                MaterialType = product.MaterialType
                            }
                            );

                    }
                    else if (updatedProduct)//否则就更新商品信息
                    {
                        var p = query.First();
                        p.Name = product.Name;
                        p.LastAccess = DateTime.Now;
                        p.Standards = product.Standards;
                        p.Remark = product.Remark;
                        p.Provider = product.Provider;
                        p.Price = product.Price;
                        p.Unit = product.Unit;
                        p.MaterialType = product.MaterialType;
                    }
                }
                entity.SaveChanges();
            }
        }


        private static Dictionary<DayOfWeek, string> cn_weekday = new Dictionary<DayOfWeek, string>
        {
            {DayOfWeek.Sunday,"周日"},
            {DayOfWeek.Monday,"周一"},
            {DayOfWeek.Tuesday,"周二"},
            {DayOfWeek.Wednesday,"周三"},
            {DayOfWeek.Thursday,"周四"},
            {DayOfWeek.Friday,"周五"},
            {DayOfWeek.Saturday,"周六"}
        };
        /// <summary>
        /// 获取分店一周的订货单,或者出库单,或者进货单
        /// </summary>
        /// <param name="node">如果为空就是返回所有分店的订货单,用于库管操作出库</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static object GetNodeOrdersList(string node, string orderType, DateTime start, DateTime end, string provider = "全部", bool combined = false)
        {
            using (var entity = new Models.DonataEntities())
            {


                var query = from s in entity.Store
                            where s.Time >= start && s.Time <= end
                            where s.StoreType == orderType
                            orderby s.Node, s.Time
                            select new
                            {
                                ID = s.ID,
                                Time = s.Time,
                                Employee = s.Employee,
                                Total = s.Total,
                                Node = s.Node,
                                Remark = s.Remark
                            };
                if (node != "总库房")
                    query = query.Where(i => i.Node == node);
                if (provider != "全部")
                    query = query.Where(i => i.Remark.Contains(provider));
                if (!combined)
                {
                    return query.ToList().Select(i => new
                    {
                        ID = i.ID,
                        Time = i.Time.ToString("yyyy-MM-dd ") + cn_weekday[i.Time.DayOfWeek],
                        Employee = i.Employee,
                        Total = i.Total,
                        Node = i.Node,
                        Remark = i.Remark
                    }
                        );
                }
                else
                {
                    var gg = from o in query.ToList()
                             group o by new { Day = o.Time.Date, Node = o.Node } into g
                             select new
                             {
                                 ID = g.Select(i => i.ID).ToList(),
                                 Time = g.First().Time.ToString("yyyy-MM-dd ") + cn_weekday[g.First().Time.DayOfWeek],
                                 Employee = g.First().Employee,
                                 Total = g.Sum(i => i.Total),
                                 Node = g.First().Node
                             };
                    return gg.ToList();

                }

            }
        }
        class Remark
        {
            public string Provider { get; set; }
        }

        public static StoreModal GetNodeOrderDetails(IEnumerable<int> orders, bool usedOrderPrice = true)
        {
            if (orders == null || orders.Count() == 0)
            {
                return new StoreModal();
            }
            using (var entity = new Models.DonataEntities())
            {
                var temp = (from detail in entity.StoreDetails
                            from p in entity.Products
                            where p.ID == detail.ProductID
                            where orders.Contains(detail.StoreID)
                            select
                            new
                            {
                                Price = p.Price,
                                Detail = new StoreDetailModal
                                {
                                    ProductID = p.ID,
                                    Amount = detail.Amount,
                                    Price = detail.Price,
                                    Name = p.Name,
                                    Remark = p.Remark,
                                    Provider = p.Provider,
                                    Standards = p.Standards,
                                    Unit = p.Unit,
                                    MaterialType = p.MaterialType
                                }
                            }).ToList();

                List<StoreDetailModal> list;
                if (usedOrderPrice)
                    list = (from s in temp
                            select s.Detail).ToList();
                else
                    list = (from s in temp
                            select
                            new StoreDetailModal
                            {
                                ProductID = s.Detail.ProductID,
                                Amount = s.Detail.Amount,
                                Price = s.Price,
                                Name = s.Detail.Name,
                                Remark = s.Detail.Remark,
                                Provider = s.Detail.Provider,
                                Standards = s.Detail.Standards,
                                Unit = s.Detail.Unit,
                                MaterialType = s.Detail.MaterialType
                            }).ToList();
                var gg = (from d in list
                          group d by new { Name = d.Name, Price = d.Price } into g //根据名称和价格分组 价格不一样的同种商品不能分到一组
                          where g.Sum(i => i.Amount) != 0
                          select new StoreDetailModal
                          {
                              Amount = g.Sum(i => i.Amount),
                              Price = g.Key.Price,
                              Name = g.Key.Name,
                              Remark = g.First().Remark,
                              Provider = g.First().Provider,
                              Standards = g.First().Standards,
                              Unit = g.First().Unit,
                              MaterialType = g.First().MaterialType,
                              ProductID = g.First().ProductID,
                              LastAccess = g.First().LastAccess
                          }).OrderBy(i => i.MaterialType).ToList();

                var targetID = orders.First();
                var store = (from s in entity.Store
                             where s.ID == targetID
                             select s).First();

                return new StoreModal
                {
                    Details = gg,
                    Employee = store.Employee,
                    Node = store.Node,
                    Remark = store.Remark,
                    StoreType = store.StoreType,
                    Time = store.Time,
                    Total = gg.Sum(i => i.Price * i.Amount).Rounding()
                };
            }
        }
        public static void DeleteNodeOrder(int orderID)
        {
            using (var entity = new Models.DonataEntities())
            {
                var queryd = from d in entity.StoreDetails
                             where d.StoreID == orderID
                             select d;
                foreach (var d in queryd)
                {
                    entity.StoreDetails.DeleteObject(d);
                }
                var querys = from s in entity.Store
                             where s.ID == orderID
                             select s;
                entity.Store.DeleteObject(querys.First());
                entity.SaveChanges();
            }
        }
        public static string GetNodeByOrderID(int id)
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from s in entity.Store
                            where s.ID == id
                            select s.Node;
                return query.First();
            }
        }

        //获取所有的商品
        public static object GetProducts(string provider = "")
        {
            using (var entity = new DonataEntities())
            {
                var queryP = from p in entity.Products
                             select new StoreDetailModal
                             {
                                 MaterialType = p.MaterialType,
                                 Unit = p.Unit,
                                 Price = p.Price,
                                 Provider = p.Provider,
                                 Remark = p.Remark,
                                 Standards = p.Standards,
                                 Name = p.Name,
                                 ProductID = p.ID
                             };
                if (!string.IsNullOrWhiteSpace(provider))
                    queryP = queryP.Where(i => i.Provider == provider);
                return queryP.ToList();
            }
        }

        //创建指令
        public static void InsertCommand(string node, string employee, string command)
        {
            using (var entity = new DonataEntities())
            {
                entity.Commands.AddObject(
                    new Commands
                    {
                        Node = node,
                        Employee = employee,
                        Time = DateTime.Now,
                        Command = command
                    }
                    );
                entity.SaveChanges();
            }
        }

        //获取指令
        public static List<CommandModal> GetCommands(string node)
        {
            using (var entity = new DonataEntities())
            {
                var query = from c in entity.Commands
                            where c.Node == node
                            select c;
                var list = new List<CommandModal>();
                query.ToList().ForEach(i =>
                {
                    list.Add(new CommandModal
                    {
                        Node = i.Node,
                        Employee = i.Employee,
                        Time = i.Time.ToString("yyyy-MM-dd HH:mm"),
                        Command = i.Command
                    });
                    entity.Commands.DeleteObject(i);
                }
                );
                entity.SaveChanges();
                return list;
            }
        }

        public static object GetProviders(string node)
        {
            if (node != "总库房") //分店目前只能从中发进货
                return new List<object> { new { Provider = "港沪中发" } };
            using (var entity = new DonataEntities())
            {
                var query = from p in entity.Products
                            where !string.IsNullOrEmpty(p.Provider)
                            select new
                            {
                                Provider = p.Provider
                            };

                return query.Distinct().ToList();

            }
        }

        public static List<StoreDetailModal> GetEastMarkOrderDetails(List<int> ids)
        {
            using (var entity = new DonataEntities())
            {
                var check = GetStock(new List<int>());
                var stock = new Dictionary<int, decimal>();
                check.ForEach(i =>
                {
                    if (i.Amount > 0)
                        stock.Add(i.ProductID, i.Amount);
                });

                var query = from d in entity.StoreDetails
                            from p in entity.Products
                            where p.ID == d.ProductID
                            where ids.Contains(d.StoreID)
                            where string.IsNullOrEmpty(p.Provider) || p.Provider == "东郊市场/西南郊市场"
                            group d by new
                            {
                                ProductID = p.ID,
                                LastAccess = p.LastAccess,
                                MaterialType = p.MaterialType,
                                Name = p.Name,
                                Price = p.Price,
                                Provider = p.Provider,
                                Remark = p.Remark,
                                Standards = p.Standards,
                                Unit = p.Unit
                            }
                                into g
                                select new StoreDetailModal
                                {
                                    ProductID = g.Key.ProductID,
                                    LastAccess = g.Key.LastAccess,
                                    MaterialType = g.Key.MaterialType,
                                    Name = g.Key.Name,
                                    Price = g.Key.Price,
                                    Provider = g.Key.Provider,
                                    Remark = g.Key.Remark,
                                    Standards = g.Key.Standards,
                                    Unit = g.Key.Unit,
                                    Amount = g.Sum(i => i.Amount)
                                };


                var result = from d in query.ToList()
                             select new StoreDetailModal
                             {
                                 ProductID = d.ProductID,
                                 LastAccess = d.LastAccess,
                                 MaterialType = d.MaterialType,
                                 Name = d.Name,
                                 Price = d.Price,
                                 Provider = d.Provider,
                                 Remark = d.Remark,
                                 Standards = d.Standards,
                                 Unit = d.Unit,
                                 Amount = stock.ContainsKey(d.ProductID) ? d.Amount - stock[d.ProductID] : d.Amount
                             };



                return result.Where(i => i.Amount > 0).ToList();

            }
        }

        /// <summary>
        /// 获取库存量
        /// </summary>
        /// <param name="orderIDs">参考订货单</param>
        /// <param name="throughput">是否计算货物的吞吐量</param>
        /// <returns></returns>
        public static List<StoreDetailModal> GetStock(List<int> orderIDs, bool throughput = false)
        {
            using (var entity = new DonataEntities())
            {
                //首先找到最后一次盘点记录
                var record = from s in entity.Store
                             where s.StoreType == "盘点"
                             orderby s.Time descending
                             select s;
                var check = record.FirstOrDefault();
                var startTime = new DateTime(1980, 3, 19);
                if (check != null)
                {
                    startTime = check.Time;
                }
                var lastmonth = DateTime.Now - new TimeSpan(30, 0, 0, 0);
                var query = from s in entity.Store
                            from d in entity.StoreDetails
                            from p in entity.Products
                            where d.ProductID == p.ID
                            where s.ID == d.StoreID
                            where s.Time >= startTime || (throughput && s.Time >= lastmonth/*如果是计算吞吐量,就从30天前开始统计*/)
                            where (s.StoreType == "进货单" && s.Node == "总库房") || s.StoreType == "出库单" || s.StoreType == "盘点"/*只取最后一次的盘点数据统计*/
                            group d by new
                            {
                                ProductID = p.ID,
                                LastAccess = p.LastAccess,
                                MaterialType = p.MaterialType,
                                Name = p.Name,
                                Price = p.Price,
                                Provider = p.Provider,
                                Remark = p.Remark,
                                Standards = p.Standards,
                                Unit = p.Unit,
                                StoreType = s.StoreType
                            }
                                into g
                                select g;


                //每个商品会有三行,一行是出库单的一组,一行进货单,一行盘点,所以需要继续分组汇总
                var list = from g in query.ToList()
                           group g by new
                           {
                               ProductID = g.Key.ProductID,
                               LastAccess = g.Key.LastAccess,
                               MaterialType = g.Key.MaterialType,
                               Name = g.Key.Name,
                               Price = g.Key.Price,
                               Provider = g.Key.Provider,
                               Remark = g.Key.Remark,
                               Standards = g.Key.Standards,
                               Unit = g.Key.Unit
                           } into gg
                           select new StoreDetailModal
                           {
                               ProductID = gg.Key.ProductID,
                               LastAccess = gg.Key.LastAccess,
                               MaterialType = gg.Key.MaterialType,
                               Name = gg.Key.Name,
                               Price = gg.Key.Price,
                               Provider = gg.Key.Provider,
                               Remark = gg.Key.Remark,
                               Standards = gg.Key.Standards,
                               Unit = gg.Key.Unit,
                               Amount = gg.Sum(g =>
                               {
                                   if (throughput) //如果是计算吞吐量
                                   {
                                       return g.Sum(i => Math.Abs(i.Amount));
                                   }
                                   var sum = g.Sum(i => i.Amount);
                                   if (g.Key.StoreType.Trim() == "出库单")
                                       return sum = -sum;
                                   return sum;
                               })
                           };

                var stock = list.OrderByDescending(i => i.LastAccess).ToList();
                if (throughput)
                    return stock;

                var details = GetNodeOrderDetails(orderIDs).Details;

                stock.ForEach(i =>
                {
                    i.Amount = i.Amount < 0 ? 0 : i.Amount;
                    var target = details.FirstOrDefault(p => p.ProductID == i.ProductID);
                    i.Need = target != null ? target.Amount : 0;
                }
                    );

                var recently = (from p in entity.Products
                                where EntityFunctions.DiffDays(DateTime.Now, p.LastAccess) < 15
                                select new StoreDetailModal
                                {
                                    Amount = 0,
                                    LastAccess = p.LastAccess,
                                    MaterialType = p.MaterialType,
                                    Name = p.Name,
                                    Need = 0,
                                    Price = p.Price,
                                    ProductID = p.ID,
                                    Provider = p.Provider,
                                    Remark = p.Remark,
                                    Standards = p.Standards,
                                    Unit = p.Unit
                                }
                                ).ToList();

                recently.ForEach(p =>
                {
                    var target = stock.FirstOrDefault(i => i.ProductID == p.ProductID);
                    if (target != null)
                    {
                        p.Amount = target.Amount;
                        p.Need = target.Need;
                    }
                }
                );



                return recently;



            }
        }
    }
}