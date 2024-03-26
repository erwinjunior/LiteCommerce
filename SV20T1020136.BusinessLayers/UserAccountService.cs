using SV20T1020136.DataLayers;
using SV20T1020136.DataLayers.SQLServer;
using SV20T1020136.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020136.BusinessLayers
{
    public static class UserAccountService
    {
        private static readonly IUserAccountDAL userAccountDB;

        static UserAccountService()
        {
            string connectionString = Configuration.ConnectionString;
            userAccountDB = new EmployeeAccountDAL(connectionString);
        }
        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static UserAccount? Authorize(string userName, string password)
        {
            return userAccountDB.Authorize(userName, password);
        }
        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public static bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            return userAccountDB.ChangePassword(userName, oldPassword, newPassword);
        }
    }
}
