using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ARDSPlatform.DAL;
using ARDSPlatform.Models;
using System.Security.Claims;
using System.Web.Security;
using MySql.Data.MySqlClient;

namespace ARDSPlatform.Controllers
{
    public class HomeController : Controller
    {
        private ards_DbContext db;
        public HomeController()
        {
            db = new ards_DbContext();
        }

        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult PlatformInfo()
        {
            try
            {
                var patients = db.tb_patients.ToList();
                Console.WriteLine($"Loaded {patients.Count} patients.");
                var cityDistribution = patients.GroupBy(p => p.City)
                   .Select(g =>
                   {
                       var dischargedPatients = g.Where(p => p.DischargeTime.HasValue).ToList();
                       TimeSpan totalStay = TimeSpan.Zero;
                       foreach (var patient in dischargedPatients)
                       {
                           if (patient.AdmissionTime.HasValue && patient.DischargeTime.HasValue)
                           {
                               totalStay = totalStay.Add(patient.DischargeTime.Value - patient.AdmissionTime.Value);
                           }
                       }
                       var averageStay = dischargedPatients.Any() ? TimeSpan.FromTicks(totalStay.Ticks / dischargedPatients.Count) : TimeSpan.Zero;
                       var deadCount = g.Count(p => p.IsDead != null && p.IsDead == "1");
                       var totalCount = g.Count();
                       return new Models.CityDistributionItem(g.Key, totalCount, $"{averageStay.Days}d {averageStay.Hours}h {averageStay.Minutes}m", deadCount, totalCount == 0 ? 0 : Math.Round((double)deadCount / totalCount * 100, 1));
                   }).ToList();
                var ageDistribution = patients.GroupBy(p => p.Age)
                   .Select(g => new AgeDistributionItem
                   {
                       Age = g.Key,
                       Count = g.Count()
                   }).ToList();
                var genderDistribution = patients.GroupBy(p => p.Gender)
                   .Select(g => new GenderDistributionItem
                   {
                       Gender = g.Key,
                       Count = g.Count()
                   }).ToList();
                var viewModel = new PlatformInfoViewModel
                {
                    PatientCount = patients.Count,
                    CityDistribution = cityDistribution,
                    AgeDistribution = ageDistribution,
                    GenderDistribution = genderDistribution,
                    AverageHospitalStay = CalculateAverageHospitalStay(patients),
                    MortalityRate = CalculateMortalityRate(patients)
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                TempData["ErrorMessage"] = "加载出现错误：" + ex.Message;
                return View("Error");
            }
        }

        public string CalculateAverageHospitalStay(List<Patient> patients)
        {
            var dischargedPatients = patients.Where(p => p.DischargeTime.HasValue).ToList();
            if (!dischargedPatients.Any())
                return "0d 0h 0m";
            TimeSpan totalStay = dischargedPatients
               .Aggregate(TimeSpan.Zero, (sum, p) =>
               {
                   if (p.DischargeTime.HasValue && p.AdmissionTime.HasValue)
                   {
                       return sum.Add(p.DischargeTime.Value - p.AdmissionTime.Value);
                   }
                   else
                   {
                       return sum;
                   }
               });
            TimeSpan averageStay = TimeSpan.FromTicks(totalStay.Ticks / dischargedPatients.Count);
            return $"{averageStay.Days}d {averageStay.Hours}h {averageStay.Minutes}m";
        }

        private double CalculateMortalityRate(List<Patient> patients)
        {
            var deadCount = patients.Count(p => p.IsDead != null && p.IsDead == "1");
            return (double)deadCount / patients.Count;
        }

        public ActionResult StatisticalAnalysis()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetStatisticalData()
        {
            try
            {
                var patients = db.tb_patients.ToList();
                Console.WriteLine($"Total patients count: {patients.Count}");
                var viewModel = new StatisticalAnalysisViewModel
                {
                    RDWData = patients.Where(p => p.RDW.HasValue).GroupBy(p => p.RDW.Value.ToString())
                       .ToDictionary(g => g.Key, g => g.Count()),
                    WBCx1000Data = patients.Where(p => p.WBCx1000.HasValue).GroupBy(p => p.WBCx1000.Value.ToString())
                       .ToDictionary(g => g.Key, g => g.Count()),
                    ChlorideData = patients.Where(p => p.Chloride.HasValue).GroupBy(p => p.Chloride.Value.ToString())
                       .ToDictionary(g => g.Key, g => g.Count()),
                    PaO2Data = patients.Where(p => p.PaO2.HasValue).GroupBy(p => p.PaO2.Value.ToString())
                       .ToDictionary(g => g.Key, g => g.Count()),
                    FiO2Data = patients.Where(p => p.FiO2.HasValue).GroupBy(p => p.FiO2.Value.ToString())
                       .ToDictionary(g => g.Key, g => g.Count()),
                    PaCO2Data = patients.Where(p => p.PaCO2.HasValue).GroupBy(p => p.PaCO2.Value.ToString())
                       .ToDictionary(g => g.Key, g => g.Count()),
                    MCHData = patients.Where(p => p.MCH.HasValue).GroupBy(p => p.MCH.Value.ToString())
                       .ToDictionary(g => g.Key, g => g.Count())
                };
                return Json(viewModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // 记录详细的错误信息到日志文件或控制台
                Console.WriteLine($"Error in GetStatisticalData: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                // 返回一个包含错误信息的 JSON 响应，以便在客户端进行错误处理
                return Json(new { error = "An error occurred while fetching statistical data. Please check the server logs for more details." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UploadData()
        {
            // 在应用程序启动时或合适的地方
            using (var context = new ards_DbContext())
            {
                context.Database.Initialize(force: true);
            }
            // 尝试重新创建数据库上下文并查询表
            using (var db = new ards_DbContext())
            {
                var uploadRecords = db.fileuploadrecords.ToList();
            }

            var existingUploadRecords = db.fileuploadrecords.ToList();
            return View("UploadData", existingUploadRecords);
        }

        public ActionResult DownloadReport()
        {
            return View();
        }
        //public FileResult DownloadReport()
        //{
        //    try
        //    {
        //        // 生成报表数据
        //        // 使用 NPOI 或其他库创建报表文件
        //        var memoryStream = new MemoryStream();
        //        // 将报表保存到内存流
        //        memoryStream.Position = 0;
        //        return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return null;
        //    }
        //}

        public ActionResult AboutUs()
        {
            return View();
        }

        public ActionResult PersonalCenter()
        {
            var viewModel = new PersonalCenterViewModel();
            return View(viewModel);
        }

        [HttpPost]
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