using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using Tools;
namespace MvcDonata.Models
{
    public class PadBusiness
    {
        //状态定义
        private static List<string> status =new List<string> { "准备","制作", "上餐", "完成" };



        /// <summary>
        /// 获取台位需要显示的餐品
        /// </summary>
        /// <param name="bar">台位名称</param>
        /// <param name="before">需要显示多少时间以前的餐品</param>
        /// <returns>返回餐品列表</returns>
        public static List<DishStatusModel> GetDishStatusByBar(string bar,TimeSpan before,int index,int maxIndex)
        {
            var startTime=DateTime.Now-before;
            var lastday=DateTime.Now-new TimeSpan(24,0,0);
            using (var entity = new DonataEntities())
            {
                var query = from s in entity.DishStatus
                            select s;
                if (bar == "出餐台")
                {

                    query = query
                        .Where(i => i.LastUpdated >= startTime);
                }
                else
                {
                    query = query.Where(i => (i.Status == "制作" || i.Status == "上餐" || i.Status == "准备") && i.Bar == bar)
                        .Where(i => (i.Status == "准备"&&i.Time>lastday) || i.LastUpdated >= startTime);
                }

                var result = (from s in query.ToList()
                              select new DishStatusModel
                                  {
                                      Bar = s.Bar,
                                      ID = s.ID,
                                      Name = s.Name,
                                      OrderBarCode = s.OrderBarCode,
                                      Remark = s.Remark,
                                      Status = s.Status,
                                      Time = s.Time.ToString("HH:mm"),
                                      CustomerCount = s.CustomerCount,
                                      LastUpdated = s.LastUpdated.ToString("HH:mm:ss"),
                                      TableNumber = s.TableNumber,
                                      TimeSpan = (int)(DateTime.Now - s.Time).TotalMinutes
                                  }).OrderBy(i => i.Time).ThenBy(i=>i.ID);
                if (index > 0)
                {
                    return result.Where(i => i.ID % maxIndex + 1 == index).ToList();
                }
                return result.ToList();

            }
        }

        public static void InsertDishStatus(IEnumerable<DishStatusModel> list)
        {
            using (var entity = new DonataEntities())
            {
                var barcode = list.First().OrderBarCode;
                var query=from d in entity.DishStatus
                          where d.OrderBarCode==barcode
                          select d;
                if(query.Count()>0) //如果此单已经加入就返回,不能重复加入
                    return;

                foreach (var dish in list)
                {
                    entity.DishStatus.AddObject(
                        new DishStatus
                        {
                            Time = DateTime.Parse(dish.Time),
                            Status = dish.Status,
                            OrderBarCode = dish.OrderBarCode,
                            Remark = dish.Remark,
                            Name = dish.Name,
                            TableNumber = dish.TableNumber,
                            Bar = dish.Bar,
                            CustomerCount = dish.CustomerCount,
                            LastUpdated = DateTime.Now
                        }
                        );
                }
                entity.SaveChanges();
            }
        }

        public static void UpdateDishStatus(int statusID, string status)
        {
            using (var entity = new DonataEntities())
            {
                var query = from d in entity.DishStatus
                            where d.ID == statusID
                            select d;
                var s=query.First();
                s.Status = status;
                s.LastUpdated = DateTime.Now;
                entity.SaveChanges();
            }
        }
        public static void ChangeTable(string barCode, string newTable)
        {
            using (var entity = new DonataEntities())
            {
                var query = from d in entity.DishStatus
                            where d.OrderBarCode == barCode
                            select d;
                foreach (var s in query)
                {
                    s.TableNumber = newTable;
                }
                entity.SaveChanges();
            }
        }
    }
}