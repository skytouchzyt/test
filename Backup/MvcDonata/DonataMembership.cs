using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tools;

namespace MvcDonata
{
    public class DonataRoleProvider : System.Web.Security.RoleProvider
    {
        public override string[] GetRolesForUser(string username)
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from e in entity.Employee
                            where e.Name == username
                            select e;
                if (query.Count() == 0)
                    return new string[0];
                else
                    return query.First().Post.Split(new char[] { ',' });
            }

        }

        #region 没有实现
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }
        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }
        public override string Description
        {
            get
            {
                return base.Description;
            }
        }
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }
        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }
        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var roles = GetRolesForUser(username);
            var targetRoles = roleName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return roles.Intersect(targetRoles).Count() > 0;
        }
        public override string Name
        {
            get
            {
                return base.Name;
            }
        }
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }
        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
    public static class DonataMembership
    {
        public static string GetUserNode(string userName)
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from e in entity.Employee
                            where e.Name == userName
                            select e;
                var user = query.First();
                return user.Node;
            }
        }
        public static Models.Employee ValidateUser(string userName, string password)
        {

            using (var entity = new Models.DonataEntities())
            {

                //使用用户名和密码登陆
                password = password.MD5();
                var query = from e in entity.Employee
                            where (e.Name == userName || e.Number == userName) && e.Password == password
                            where e.Actived
                            select e;
                if (query.Count() > 0)
                {
                    query.First().LastLogin = DateTime.Now;//设置最后一次登录时间
                    entity.SaveChanges();
                    return query.First();
                }
                else
                    return null;
            }
        }
        public static bool HasUser(string name)
        {
            using (var entity = new Models.DonataEntities())
            {
                var query = from e in entity.Employee
                            where e.Name == name
                            select e;
                return query.Count() > 0;
            }

        }
        public static void ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (ValidateUser(userName, oldPassword) == null)
                throw new Exception("旧密码错误.");
            using (var entity = new Models.DonataEntities())
            {
                var query = from e in entity.Employee
                            where e.Name == userName
                            select e;
                var user = query.First();
                user.Password = newPassword.MD5();


                entity.SaveChanges();
            }
        }
        public static void CreateUser(Models.EmployeeModel reg)
        {
            if (HasUser(reg.Name))
                throw new Exception("此员工已经录入.");
            using (var entity = new Models.DonataEntities())
            {
                var e = reg.Convert<Models.Employee>();
                e.Password = "123456".MD5();

                //var e = new Models.Employee
                //{
                //    Name = reg.Name,
                //    Password = "123456".MD5(),//初始化密码为123456
                //    Actived = reg.Actived,
                //    Node = reg.Node,
                //    Post = reg.Post,
                //    Salary = reg.Salary,
                //    EntryDate = reg.EntryDate,
                //    Phone = reg.Phone,
                //    Number = reg.Number,
                //    BankCard = reg.BankCard,

                //};
                entity.Employee.AddObject(e);
                entity.SaveChanges();
            }
        }

    }
}