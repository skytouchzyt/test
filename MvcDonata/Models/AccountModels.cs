using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace MvcDonata.Models
{

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "旧密码")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 至少需要{2}位.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "再输入一次")]
        [Compare("NewPassword", ErrorMessage = "两次密码输入不一致.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "姓名",Description="账号名")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码",Description="账号密码")]
        public string Password { get; set; }

        [Display(Name = "记住我",Description="记住用户名和密码,下次自动登录")]
        public bool RememberMe { get; set; }
    }

    public class EmployeeModel
    {

        [Display(Name = "ID")]
        public int ID { get; set; }

        [Required]
        [Display(Name = "姓名")]
        public string Name { get; set; }

        [Display(Name = "编号")]
        [StringLength(20, ErrorMessage = "卡号必须是{2}为以上.", MinimumLength = 8)]
        public string Number { get; set; }

 

        [Display(Name = "银行卡号")]
        public string BankCard { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "入职时间")]
        [DisplayFormat(DataFormatString = @"{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EntryDate { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "职位")]
        public string Post { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "工资")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = true)]
        public decimal Salary { get; set; }

        [Required]
        [Display(Name = "所属分店")]
        public string Node { get; set; }


        [Display(Name = "电话")]
        public string Phone { get; set; }


        [Display(Name = "激活")]
        public bool Actived { get; set; }




    }
}
