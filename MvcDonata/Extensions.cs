using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace MvcDonata
{
    public static class Extensions
    {

        public static string GetString(this string[] strs)
        {
            if (strs.Length == 0)
                return "";
            var str = strs[0];
            for (var i = 1; i < strs.Length; i++)
            {
                str += "," + strs[i];
            }
            return str;
        }

        public static T Convert<T>(this object a, object b=null) where T:new()
        {
            if (b == null) b = new T();

          

            var queryA = (from p in a.GetType().GetProperties()
                        select p.Name).ToList();
            var queryB = (from p in b.GetType().GetProperties()
                         select p.Name).ToList();


            var con = queryA.Intersect(queryB);

            foreach (var p in con)
            {
                var v = a.GetType().GetProperty(p).GetValue(a, null);
                if (v == null)
                    continue;
                b.GetType().GetProperty(p).SetValue(b, a.GetType().GetProperty(p).GetValue(a, null), null);
            }

            return (T)b;

        }
    }
}