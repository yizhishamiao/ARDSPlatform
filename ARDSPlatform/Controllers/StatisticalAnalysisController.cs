using System;
using System.Linq;
using System.Web.Mvc;
using ARDSPlatform.DAL;
using ARDSPlatform.Models;

namespace ARDSPlatform.Controllers
{
    public class StatisticalAnalysisController : Controller
    {
        private readonly ards_DbContext _db;

        public StatisticalAnalysisController()
        {
            _db = new ards_DbContext();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetStatisticalData()
        {
            try
            {
                var patients = _db.tb_patients.ToList();
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
    }
}
