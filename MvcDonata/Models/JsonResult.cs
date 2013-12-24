using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcDonata.Models
{
    public class iJsonResult
    {
        public object result { get; set; }
        public string errorMessage { get; set; }
        public string MD5 { get; set; }

        public iJsonResult(object r,string message=null)
        {
            result = r;
            errorMessage = null;
        }
    }
}