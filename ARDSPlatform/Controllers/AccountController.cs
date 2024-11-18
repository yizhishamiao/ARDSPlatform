using System.Web.Mvc;
using ARDSPlatform.Models;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Data.Entity;
using ARDSPlatform.DAL;
using System.Web.Security;
using System.Net;
using System.Net.Sockets;


namespace ARDSPlatform.Controllers
{
    public class AccountController : Controller
    {
        private ards_DbContext db = new ards_DbContext();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Login(string Account, string Password)
        {
            var user = db.tb_users.FirstOrDefault(u => u.Account == Account && u.Password == Password);
            if (user != null)
            {
                // 登录成功，记录登录历史
                Console.WriteLine("用户登录成功，准备记录登录历史。");
                var loginTime = DateTime.Now;
                var ipAddress = GetClientIpAddress();
                LogUserLogin(user.UserId, ipAddress, loginTime);
                // 设置身份验证 cookie
                FormsAuthentication.SetAuthCookie(Account, false);
                Session["IsLoggedIn"] = true;
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "账号或密码错误！" });
            }
        }


        public string GetClientIpAddress()
        {
            string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ipAddress) || ipAddress.ToLower() == "unknown")
            {
                ipAddress = Request.ServerVariables["HTTP_X_REAL_IP"];
            }

            if (string.IsNullOrEmpty(ipAddress) || ipAddress.ToLower() == "unknown")
            {
                ipAddress = Request.ServerVariables["REMOTE_ADDR"];
            }

            if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(","))
            {
                ipAddress = ipAddress.Split(',').FirstOrDefault(); // 获取第一个非空的 IP 地址
            }

            if (IPAddress.TryParse(ipAddress, out IPAddress parsedIpAddress))
            {
                // 如果是 IPv6 并且可以映射为 IPv4，则转换为 IPv4
                if (parsedIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    if (parsedIpAddress.Equals(IPAddress.IPv6Loopback))
                    {
                        ipAddress = IPAddress.Loopback.ToString(); // 将 ::1 转换为 127.0.0.1
                    }
                    else if (parsedIpAddress.IsIPv4MappedToIPv6)
                    {
                        ipAddress = parsedIpAddress.MapToIPv4().ToString();
                    }
                }
            }

            return ipAddress;
        }


        private bool IsPrivateIpAddress(string ipAddress)
        {
            // 私有 IP 地址范围
            string[] privateIpRanges = { "10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16", "127.0.0.0/8" };
            // 检查 IP 地址是否在私有 IP 地址范围内
            foreach (string range in privateIpRanges)
            {
                if (ipAddress.StartsWith(range.Split('/')[0]))
                {
                    return true;
                }
            }
            return false;
        }







        private void LogUserLogin(int userId, string userIp, DateTime loginTime)
        {
            Console.WriteLine($"进入 LogUserLogin 方法，UserId: {userId}, IP: {userIp}, LoginTime: {loginTime}");

            // 去除IP地址可能存在的首尾空格
            userIp = userIp.Trim();
            if (userIp.Length > 45)
            {
                userIp = userIp.Substring(0, 45);
                Console.WriteLine($"Truncated IP address to fit varchar(45) limit: {userIp}");
            }
            var loginHistory = new TbLoginHistory
            {
                UserId = userId,
                LoginIp = userIp,
                LoginTime = loginTime
            };
            db.tb_login_history.Add(loginHistory);
            Console.WriteLine("准备保存登录历史记录到数据库");
            // 检查数据库连接是否可用
            if (db.Database.Connection.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("数据库连接不可用，尝试重新打开连接...");
                db.Database.Connection.Open();
            }
            // 确保只保留最新的 5 条登录历史记录
            var allHistory = db.tb_login_history
                .Where(lh => lh.UserId == userId)
                .OrderByDescending(lh => lh.LoginTime)
                .ToList();


            try
            {
                db.SaveChanges(); // 确保提交数据库更改
                Console.WriteLine("登录历史记录已存储。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存登录历史记录时出错: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常: {ex.InnerException.Message}");
                }
            }

        }

        
    }
}