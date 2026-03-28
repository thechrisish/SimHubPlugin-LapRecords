using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SimHubLapRecordPlugin.Models;

namespace SimHubLapRecordPlugin
{
    public class Settings
    {
        // TrackName -> (CarName -> LapRecord)
        public Dictionary<string, Dictionary<string, LapRecord>> TrackRecords { get; set; } = new Dictionary<string, Dictionary<string, LapRecord>>();
        
        // OriginalCarName -> OverriddenCarName
        public Dictionary<string, string> CarNameOverrides { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, GameTyreOverride> GameTyreOverrides { get; set; } = new Dictionary<string, GameTyreOverride>();

        // Legacy properties to prevent JSON deserialization crashes on old files
        public List<string> CustomTyreCompounds { get; set; }
        public string TyrePropertyOverrideFL { get; set; }
        public string TyrePropertyOverrideFR { get; set; }
        public string TyrePropertyOverrideRL { get; set; }
        public string TyrePropertyOverrideRR { get; set; }

        public ObservableCollection<TyreCompoundDef> TyreCompoundDefinitions { get; set; } = new ObservableCollection<TyreCompoundDef>();

        public Dictionary<string, bool> ColumnVisibility { get; set; } = new Dictionary<string, bool>
        {
            { "Date", true },
            { "Game", true },
            { "Car", true },
            { "Class", true },
            { "Session", true },
            { "Fuel Level", true },
            { "Track Temp", true },
            { "Track State", true },
            { "Tyre", true },
            { "Laptime", true }
        };
    }

    public class TyreCompoundDef
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string BackgroundColor { get; set; }

        public override string ToString() => Name;
    }

    public class GameTyreOverride
    {
        public string OverrideFL { get; set; } = "";
        public string OverrideFR { get; set; } = "";
        public string OverrideRL { get; set; } = "";
        public string OverrideRR { get; set; } = "";
    }
}
