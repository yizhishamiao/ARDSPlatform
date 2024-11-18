using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ARDSPlatform.DAL;
using ARDSPlatform.Models;
using System.Security.Claims;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using MySql.Data.MySqlClient;

namespace ARDSPlatform.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PersonalCenterController : Controller
    {
        private ards_DbContext db;

        public PersonalCenterController()
        {
            db = new ards_DbContext();
        }

        // GET: PersonalCenter
        public ActionResult Index()
        {
            var viewModel = new PersonalCenterViewModel();
            return View(viewModel);
        }

        // 获取登录历史
        public JsonResult GetLoginHistory()
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                Console.WriteLine($"Current user ID: {currentUserId}");

                // 查询最新的5条登录记录
                var query = "SELECT LoginIp, LoginTime FROM tb_login_history WHERE UserId = @UserId ORDER BY LoginTime DESC LIMIT 5";
                var loginHistory = db.Database.SqlQuery<PersonalCenterViewModel>(query, new MySqlParameter("@UserId", currentUserId)).ToList();

                Console.WriteLine($"登录历史记录数: {loginHistory.Count}");  // 输出查询结果数量

                // 格式化登录时间为字符串
                var formattedHistory = loginHistory.Select(lh => new
                {
                    loginIP = lh.LoginIp,
                    loginTime = lh.LoginTime.ToString("yyyy-MM-dd HH:mm:ss") // 格式化为所需格式
                }).ToList();

                Console.WriteLine($"Login history count: {formattedHistory.Count}");
                return Json(formattedHistory, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLoginHistory: {ex.Message}");
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }



        // 获取当前用户 ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.Identity.Name;
            Console.WriteLine($"User identity name: {userIdClaim}");
            var userId = db.tb_users.FirstOrDefault(u => u.Account == userIdClaim)?.UserId ?? 0;
            Console.WriteLine($"Current user ID: {userId}");
            return userId;
        }



        // 修改密码
        [System.Web.Mvc.HttpPost]
        public JsonResult ChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                // 获取当前登录用户
                var user = db.tb_users.FirstOrDefault(u => u.Account == User.Identity.Name);
                if (user == null)
                {
                    return Json(new { success = false, message = "用户未找到！" });
                }
                // 验证旧密码
                if (user.Password != oldPassword)
                {
                    return Json(new { success = false, message = "原密码错误！" });
                }
                // 检查新密码是否与旧密码相同
                if (user.Password == newPassword)
                {
                    return Json(new { success = false, message = "新密码不可和原密码相同！" });
                }
                // 更新密码
                user.Password = newPassword;
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ChangePassword: {ex.Message}");
                return Json(new { success = false, message = "密码修改失败：" + ex.Message });
            }
        }
    }
}