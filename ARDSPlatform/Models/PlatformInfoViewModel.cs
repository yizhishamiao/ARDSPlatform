using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ARDSPlatform.Models
{
    public class CityDistributionItem
    {
        public string City { get; }
        public int Count { get; }
        public string AverageStay { get; }
        public int DeadCount { get; }
        public double MortalityRateValue { get; }

        public CityDistributionItem(string city, int count, string averageStay, int deadCount, double mortalityRateValue)
        {
            City = city;
            Count = count;
            AverageStay = averageStay;
            DeadCount = deadCount;
            MortalityRateValue = mortalityRateValue;
        }
    }







    public class AgeDistributionItem
    {
        public int Age { get; set; }
        public int Count { get; set; }
    }

    public class GenderDistributionItem
    {
        public string Gender { get; set; }
        public int Count { get; set; }
    }

    public class PlatformInfoViewModel
    {
        public int PatientCount { get; set; }
        public List<CityDistributionItem> CityDistribution { get; set; }
        public List<AgeDistributionItem> AgeDistribution { get; set; }
        public List<GenderDistributionItem> GenderDistribution { get; set; }
        public string AverageHospitalStay { get; set; }
        public double MortalityRate { get; set; }
        public List<Patient> PatientsForView { get; set; }
    }
}
