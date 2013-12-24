using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using Tools;

using Models.MongoDB;


using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;

namespace MvcDonata.Models
{
    public class PadBusiness
    {
        //状态定义
        private static List<string> status =new List<string> { "准备","制作", "上餐", "完成" };





        public static List<DishStatus> GetDishStatusByBar_mongodb(string bar, TimeSpan before, int index, int maxIndex)
        {
            var startTime = (DateTime.Now - before).ToString(BusinessObject.datetimeFormatString);
            var lastday = (DateTime.Now - new TimeSpan(24, 0, 0)).ToString(BusinessObject.datetimeFormatString);

            var db = BusinessObject.mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<DishStatus>("DishStatus");

            IMongoQuery query;
                if (bar == "出餐台")
                {

                    query = Query.GT("LastUpdated",startTime);
                }
                else
                {
                    query = Query.And(Query.EQ("Bar", bar), Query.In("Status", new List<BsonValue> { BsonValue.Create("制作"), BsonValue.Create("上餐"), BsonValue.Create("准备") })) ;
                    var q1=Query.And(Query.EQ("Status","准备"),Query.GT("Time",lastday));
                    var q2=Query.GTE("LastUpdated",startTime);
                    var q = Query.Or(q1, q2);
                    query = Query.And(query, q);
                }

                var r = collection.Find(query);
               
                var result = (from s in r.ToList()
                              select new DishStatus
                              {
                                  _id=new BsonObjectId(s.ID),
                                  Bar = s.Bar,
                                  Name = s.Name,
                                  OrderBarCode = s.OrderBarCode,
                                  Remark = s.Remark,
                                  Status = s.Status,
                                  Time = DateTime.Parse(s.Time).ToString("HH:mm"),
                                  CustomerCount = s.CustomerCount,
                                  LastUpdated = DateTime.Parse(s.LastUpdated).ToString("HH:mm:ss"),
                                  TableNumber = s.TableNumber,
                                  TimeSpan = (int)(DateTime.Now - DateTime.Parse(s.Time)).TotalMinutes
                              }).OrderBy(i => i.Time).ThenBy(i => i.ID);

                return result.ToList();

            
        }


    
        public static void InsertDishStatus(IEnumerable<DishStatus> list)
        {
            var barcode = list.First().OrderBarCode;

            var db = BusinessObject.mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<DishStatus>("DishStatus");
            var query = from s in collection.AsQueryable()
                        where s.OrderBarCode == barcode
                        select s;
            if (query.Count() > 0)
                return;

            foreach (var dish in list)
            {
                collection.Insert(
                    new DishStatus
                        {
                            Time = dish.Time,
                            Status = dish.Status,
                            OrderBarCode = dish.OrderBarCode,
                            Remark = dish.Remark,
                            Name = dish.Name,
                            TableNumber = dish.TableNumber,
                            Bar = dish.Bar,
                            CustomerCount = dish.CustomerCount,
                            LastUpdated = DateTime.Now.ToString(BusinessObject.datetimeFormatString)
                        },SafeMode.True);
            }

        }



        public static void UpdateDishStatus(string statusID, string status)
        {
            var db = BusinessObject.mongodbAddress.MongoDB("donata");
            var collection = db.GetCollection<DishStatus>("DishStatus");

            var query = Query.EQ("_id", new  BsonObjectId(statusID));

            var s = collection.FindOne(query);



            s.LastUpdated = DateTime.Now.ToString(BusinessObject.datetimeFormatString);
            s.Status = status;

            var bd = BsonExtensionMethods.ToBsonDocument(s);

            collection.Update(query, new UpdateDocument(bd), SafeMode.True);
            

        }





        public static void ChangeTable(string barcode, string newTable)
        {
            var db = BusinessObject.mongodbAddress.MongoDB("donata");
            var collecton = db.GetCollection<DishStatus>("DishStatus");

            var query = Query.EQ("OrderBarCode", barcode);
            var update = Update.Set("TableNumber", newTable);
            collecton.Update(query, update,UpdateFlags.Multi,SafeMode.True);//更新多条数据

        }
    }
}