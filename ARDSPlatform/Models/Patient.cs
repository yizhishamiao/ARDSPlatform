using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ARDSPlatform.Models
{
    [Table("tb_patients")]
    public class Patient
    {
        public int PatientId { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string Label { get; set; }
        public double? RDW { get; set; }
        public double? WBCx1000 { get; set; }
        public double? Chloride { get; set; }
        public double? PaCO2 { get; set; }
        public double? FiO2 { get; set; }
        public double? PaO2 { get; set; }
        public double? MCH { get; set; }
        public string City { get; set; }
        public DateTime? AdmissionTime { get; set; }
        public DateTime? DischargeTime { get; set; }
        public string IsDead { get; set; }
    }
}