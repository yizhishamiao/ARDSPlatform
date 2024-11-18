using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ARDSPlatform.DAL;
using ARDSPlatform.Models;
using Newtonsoft.Json;

namespace ARDSPlatform.Controllers
{
    public class UploadDataController : Controller
    {
        private ards_DbContext db;

        public UploadDataController()
        {
            db = new ards_DbContext();
            try
            {
                if (db.Database.Exists())
                {
                    Console.WriteLine("数据库连接正常。");
                }
                else
                {
                    Console.WriteLine("数据库连接异常。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库连接错误：{ex.Message}");
            }
        }

        [HttpGet]
        public ActionResult Upload()
        {
            var uploadRecords = db.fileuploadrecords.ToList();
            return View(uploadRecords);
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            Console.WriteLine("进入 Upload 方法，接收到的文件名为：" + file.FileName);

            if (file.ContentLength == 0)
            {
                Console.WriteLine("文件大小为0，可能存在问题");
            }

            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                Console.WriteLine("获取到的文件名：" + fileName);
                var fileType = Path.GetExtension(fileName);
                var uploadTime = DateTime.Now;
                var uploadDir = Server.MapPath("~/App_Data/uploads");
                Console.WriteLine("计算得到的上传目录：" + uploadDir);
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir); // 如果目录不存在，创建该目录
                }
                var path = Path.Combine(uploadDir, fileName);

                Console.WriteLine("实际文件保存路径：" + path);

                try
                {
                    // 获取文件名不带后缀并存储在一个临时变量中
                    var fileNameWithoutExtensionTemp = Path.GetFileNameWithoutExtension(fileName);

                    // 检查文件名是否已存在（使用临时变量在内存中进行比较）
                    if (db.fileuploadrecords.Any(f => f.FileName == fileNameWithoutExtensionTemp))
                    {
                        return Json(new { success = false, message = "该文件名已存在！" });
                    }


                    // 限制文件大小为 10MB
                    if (file.ContentLength > 10 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = "文件过大，最大支持 10MB！" });
                    }

                    Console.WriteLine("即将保存文件到路径: " + path);

                    try
                    {
                        file.SaveAs(path);
                        Console.WriteLine("文件保存成功");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("文件保存失败，异常信息: " + ex.Message);
                        return Json(new { success = false, message = "文件上传失败，请稍后重试。具体错误：" + ex.Message });
                    }

                    var fileUploadRecord = new FileUploadRecord
                    {
                        FileName = fileNameWithoutExtensionTemp,
                        FileType = fileType,
                        UploadTime = uploadTime
                    };
                    Console.WriteLine($"准备保存的实体对象：FileName={fileUploadRecord.FileName}, FileType={fileUploadRecord.FileType}, UploadTime={fileUploadRecord.UploadTime}");
                    db.fileuploadrecords.Add(fileUploadRecord);
                    db.SaveChanges();
                    return Json(new { success = true, message = "文件上传成功", fileName = fileNameWithoutExtensionTemp, fileType = fileType, uploadTime = uploadTime.ToString("yyyy-MM-ddTHH:mm:ssZ") });
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"内部异常信息：{ex.InnerException.Message}");
                        Console.WriteLine(ex.InnerException.StackTrace);
                    }
                    return Json(new { success = false, message = "文件上传失败，请稍后重试。具体错误：" + ex.Message });
                }
            }
            else
            {
                return Json(new { success = false, message = "未选择文件或文件大小为 0！" });
            }
        }

        private const int recordsPerPage = 10;
        public JsonResult GetUploadHistory(int page)
        {
            try
            {
                var skip = (page - 1) * recordsPerPage;
                var history = db.fileuploadrecords.OrderByDescending(f => f.FileId).Skip(skip).Take(recordsPerPage).ToList();
                if (!history.Any())
                {
                    return Json(new List<FileUploadRecord>(), JsonRequestBehavior.AllowGet);
                }
                return Json(history, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUploadHistory: {ex.Message}");
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetTotalUploadRecords()
        {
            try
            {
                return Json(db.fileuploadrecords.Count(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // 记录错误信息
                Console.WriteLine($"Error in file save: {ex.Message}");
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CheckFileNameExists(string fileNameWithoutExtension)
        {
            var exists = db.fileuploadrecords.Any(f => f.FileName == fileNameWithoutExtension);
            return Json(exists, JsonRequestBehavior.AllowGet);
        }
    }
}
