using System.Collections.Generic;

namespace ARDSPlatform.Models
{
    public class StatisticalAnalysisViewModel
    {
        public Dictionary<string, int> RDWData { get; set; }
        public Dictionary<string, int> WBCx1000Data { get; set; }
        public Dictionary<string, int> ChlorideData { get; set; }
        public Dictionary<string, int> PaO2Data { get; set; }
        public Dictionary<string, int> FiO2Data { get; set; }
        public Dictionary<string, int> PaCO2Data { get; set; }
        public Dictionary<string, int> MCHData { get; set; }

        public StatisticalAnalysisViewModel()
        {
            RDWData = new Dictionary<string, int>();
            WBCx1000Data = new Dictionary<string, int>();
            ChlorideData = new Dictionary<string, int>();
            PaO2Data = new Dictionary<string, int>();
            FiO2Data = new Dictionary<string, int>();
            PaCO2Data = new Dictionary<string, int>();
            MCHData = new Dictionary<string, int>();
        }
    }
}