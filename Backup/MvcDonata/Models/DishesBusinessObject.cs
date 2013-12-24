using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcDonata.Models
{
    public class DishesBusinessObject
    {

        public static List<DishModel> GetDishes()
        {
            using (var entity = new DonataEntities())
            {
                var query = from d in entity.Dishes
                            select new DishModel
                            {
                                CanDiscounted = d.CanDiscounted,
                                ClassName = d.ClassName,
                                Count = d.Count,
                                DefaultBar = d.DefaultBar,
                                ID = d.ID,
                                Name = d.Name,
                                Price = d.Price
                            };
                return query.ToList();
            }
        }
        public static void Insert(DishModel dish)
        {

        }
    }
}