using System;

namespace SimHubLapRecordPlugin.Models
{
    public class LapRecord
    {
        public string GameName { get; set; } = "Unknown";
        public string CarName { get; set; }
        private string _carClass;
        public string CarClass 
        { 
            get => _carClass == "Unknown" ? "" : _carClass;
            set => _carClass = value;
        }

        private string _session;
        public string Session 
        { 
            get => _session == "Unknown" ? "" : _session;
            set => _session = value;
        }
        public TimeSpan LapTime { get; set; }
        public string TyreCompound { get; set; }
        public string TyreCompoundFL { get; set; }
        public string TyreCompoundFR { get; set; }
        public string TyreCompoundRL { get; set; }
        public string TyreCompoundRR { get; set; }
        private string _trackTemperature;
        public string TrackTemperature 
        { 
            get => _trackTemperature == "0.0°C" || _trackTemperature == "0.0°F" || _trackTemperature == "Unknown" ? "" : _trackTemperature;
            set => _trackTemperature = value;
        }

        private string _trackState;
        public string TrackState 
        { 
            get => string.IsNullOrWhiteSpace(_trackState) ? "Unknown" : _trackState;
            set => _trackState = value;
        }
        public double FuelLevel { get; set; } = 0.0;
        public string FuelUnit { get; set; } = "L";
        public DateTime RecordDate { get; set; } = DateTime.Now;

        /// <summary>
        /// The raw car name as reported by the sim, before any CarNameOverride is applied.
        /// Set once on record creation; never modified by override logic.
        /// </summary>
        public string OriginalCarName { get; set; }

        /// <summary>
        /// The raw track name as reported by the sim, before any TrackNameOverride is applied.
        /// Set once on record creation; never modified by merge logic.
        /// </summary>
        public string OriginalTrackName { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string DisplayFuel => FuelLevel > 0 ? $"{FuelLevel:F1} {FuelUnit}" : "";
    }
}
