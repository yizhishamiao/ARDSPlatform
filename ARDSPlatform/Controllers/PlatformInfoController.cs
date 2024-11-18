using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ARDSPlatform.DAL;
using ARDSPlatform.Models;

namespace ARDSPlatform.Controllers
{
    public class PlatformInfoController : Controller
    {
        private readonly ards_DbContext _db;

        public PlatformInfoController(ards_DbContext db)
        {
            _db = db;
        }

        public ActionResult Index()
        {
            var viewModel = PrepareViewModel();
            return View(viewModel);
        }

        private PlatformInfoViewModel PrepareViewModel()
        {
            var patients = _db.tb_patients.ToList();
            var cityGroupedPatients = patients.GroupBy(p => p.City);

            var cityDistribution = cityGroupedPatients.Select(g =>
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
                MortalityRate = CalculateMortalityRate(patients),
                PatientsForView = patients
            };

            return viewModel;
        }

        public string CalculateAverageHospitalStay(List<Patient> patients)
        {
            var cityGroupedPatients = patients.GroupBy(p => p.City);
            var result = new Dictionary<string, string>();
            foreach (var group in cityGroupedPatients)
            {
                var dischargedPatients = group.Where(p => p.DischargeTime.HasValue).ToList();
                TimeSpan totalStay = TimeSpan.Zero;
                foreach (var patient in dischargedPatients)
                {
                    if (patient.AdmissionTime.HasValue && patient.DischargeTime.HasValue)
                    {
                        totalStay = totalStay.Add(patient.DischargeTime.Value - patient.AdmissionTime.Value);
                    }
                }
                var averageStay = dischargedPatients.Any() ? TimeSpan.FromTicks(totalStay.Ticks / dischargedPatients.Count) : TimeSpan.Zero;
                result[group.Key] = $"{averageStay.Days}d {averageStay.Hours}h {averageStay.Minutes}m";
            }
            return string.Join(", ", result.Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        private double CalculateMortalityRate(List<Patient> patients)
        {
            var cityGroupedPatients = patients.GroupBy(p => p.City);
            var result = new Dictionary<string, double>();
            foreach (var group in cityGroupedPatients)
            {
                var deadCount = group.Count(p => p.IsDead != null && p.IsDead == "1");
                var totalCount = group.Count();
                result[group.Key] = totalCount == 0 ? 0 : Math.Round((double)deadCount / totalCount * 100, 1);
            }
            return result.Average(kv => kv.Value);
        }
    }
}
