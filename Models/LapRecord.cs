using System;

namespace SimHubLapRecordPlugin.Models
{
    public class LapRecord
    {
        public string GameName { get; set; } = "Unknown";
        public string CarName { get; set; }
        public string CarClass { get; set; }
        public string Session { get; set; }
        public TimeSpan LapTime { get; set; }
        public string TyreCompound { get; set; }
        public string TyreCompoundFL { get; set; }
        public string TyreCompoundFR { get; set; }
        public string TyreCompoundRL { get; set; }
        public string TyreCompoundRR { get; set; }
        public string TrackTemperature { get; set; }
        public string TrackState { get; set; }
        public double FuelLevel { get; set; } = 0.0;
        public string FuelUnit { get; set; } = "L";
        public DateTime RecordDate { get; set; } = DateTime.Now;

        [Newtonsoft.Json.JsonIgnore]
        public string DisplayFuel => $"{FuelLevel:F1} {FuelUnit}";
    }
}
