using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace MvcDonata.Models
{
    public class ProductModel
    {
        [Required]
        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "规格")]
        public string Standard { get; set; }

        [Required]
        [Display(Name = "单位")]
        public string Unit { get; set; }

        [Required]
        [Display(Name = "单价")]
        public decimal Price { get; set; }

        [Display(Name = "供货商")]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "类别")]
        public string ClassName { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Required]
        [Display(Name = "激活")]
        public bool Actived { get; set; }
    }

    public enum OrderType
    {
        出库,
        入库
    }

    public class OrderModel
    {

        public int OrderID { get; set; }

        [Required]
        [Display(Name = "时间")]
        public DateTime DateTime { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        [Required]
        public string Node { get; set; }

        [Required]
        [Display(Name = "总金额")]
        public decimal Total { get; set; }

        [Required]
        [Display(Name = "交易对象")]
        public string Target { get; set; }

        [Required]
        [Display(Name = "类型")]
        public int OrderType { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }
    }
}