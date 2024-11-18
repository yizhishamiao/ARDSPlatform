using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ARDSPlatform.Models;
using ARDSPlatform.DAL;

namespace ARDSPlatform.Controllers
{
    public class DownloadReportController : Controller
    {
        private ards_DbContext db;
        public DownloadReportController()
        {
            db = new ards_DbContext();
        }

        // GET: DownloadReport
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QueryData(string gender, string ageRange, string outcome, string[] features)
        {
            // 解析年龄范围字符串为起始年龄和结束年龄
            var ageRangeParts = ageRange.Split(new string[] { "——" }, StringSplitOptions.None);
            int startAge = int.Parse(ageRangeParts[0]);
            int endAge = int.Parse(ageRangeParts[1]);

            var query = db.tb_patients.AsQueryable();

            // 根据性别条件筛选
            if (!string.IsNullOrEmpty(gender) && gender != "全部")
            {
                query = query.Where(p => p.Gender == gender);
            }

            // 根据年龄条件筛选
            query = query.Where(p => p.Age >= startAge && p.Age <= endAge);

            // 根据结局条件筛选
            if (!string.IsNullOrEmpty(outcome) && outcome != "全部")
            {
                query = query.Where(p => p.IsDead == outcome);
            }

            // 根据特征条件筛选
            if (features != null && features.Length > 0)
            {
                query = query.Where(p => features.Contains(p.Label));
            }

            var result = query.ToList();

            // 将查询结果传递给视图进行展示
            return View("ResultView", result);
        }

        [HttpGet]
        public FileResult DownloadFile(int fileId)
        {
            var fileRecord = db.fileuploadrecords.FirstOrDefault(f => f.FileId == fileId);
            if (fileRecord == null)
            {
                return null;
            }

            // 构建完整的文件物理路径
            var physicalPath = Server.MapPath("~/App_Data/uploads" + "/" + fileRecord.FileName);
            var fileBytes = System.IO.File.ReadAllBytes(physicalPath);

            return File(fileBytes, fileRecord.FileType, fileRecord.FileName);
        }
    }
}