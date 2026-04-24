using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using GameReaderCommon;
using SimHub.Plugins;
using SimHubLapRecordPlugin.Models;

namespace SimHubLapRecordPlugin
{
    [PluginDescription("Logs optimal lap times per track & car combination.")]
    [PluginAuthor("antigravity")]
    [PluginName("Lap Record Plugin")]
    public class LapRecordPlugin : IPlugin, IDataPlugin, IWPFSettingsV2
    {
        public PluginManager PluginManager { get; set; }
        public Settings Settings;

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(LapRecordPlugin));

        // Per-frame caching to avoid redundant string allocations (3.3)
        private string _lastTrackName = null;
        private string _lastCarName = null;
        private string _lastTrackCarKey = null;

        public string LeftMenuTitle => "Lap Records";

        public ImageSource PictureIcon
        {
            get
            {
                var geometry = Geometry.Parse("M15,1H9V3H15V1M11,14H13V8H11V14M19.03,7.39L20.45,5.97C20,5.46 19.55,5 19.04,4.56L17.62,5.98C16.07,4.74 14.12,4 12,4C7.03,4 3,8.03 3,13C3,17.97 7.03,22 12,22C16.97,22 21,17.97 21,13C21,10.88 20.26,8.93 19.03,7.39M12,20C8.13,20 5,16.87 5,13C5,9.13 8.13,6 12,6C15.87,6 19,9.13 19,13C19,16.87 15.87,20 12,20Z");
                return new DrawingImage(new GeometryDrawing(Brushes.White, new Pen(Brushes.White, 0), geometry));
            }
        }

        public void Init(PluginManager pluginManager)
        {
            // 1.1: Use SimHub's native settings persistence (IPlugin extension method)
            Settings = this.ReadCommonSettings<Settings>("Settings", () => new Settings());

            if (Settings.TyreCompoundDefinitions == null || Settings.TyreCompoundDefinitions.Count == 0)
            {
                Settings.TyreCompoundDefinitions = new System.Collections.ObjectModel.ObservableCollection<TyreCompoundDef>
                {
                    new TyreCompoundDef { Name = "Soft",         Abbreviation = "S",  BackgroundColor = "White"  },
                    new TyreCompoundDef { Name = "Medium",       Abbreviation = "M",  BackgroundColor = "Yellow" },
                    new TyreCompoundDef { Name = "Hard",         Abbreviation = "H",  BackgroundColor = "Red"    },
                    new TyreCompoundDef { Name = "Intermediate", Abbreviation = "I",  BackgroundColor = "Green"  },
                    new TyreCompoundDef { Name = "Wet",          Abbreviation = "W",  BackgroundColor = "Blue"   }
                };
            }

            pluginManager.AddProperty("CurrentCarBestLap", this.GetType(), "-");
            pluginManager.AddProperty("CurrentCarBestLapTime", this.GetType(), 0.0);
        }

        public Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return new SettingsControl(this);
        }

        public void End(PluginManager pluginManager)
        {
            SaveSettings();
        }

        public void SaveSettings()
        {
            // 1.1 + 3.2: Native SimHub persistence via IPlugin extension method
            this.SaveCommonSettings("Settings", Settings);
        }

        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (!data.GameRunning || data.NewData == null) return;

            string trackName = data.NewData.TrackName ?? "UnknownTrack";
            string carName   = data.NewData.CarModel  ?? "UnknownCar";

            if (Settings.CarNameOverrides.TryGetValue(carName, out var overridden) && !string.IsNullOrWhiteSpace(overridden))
                carName = overridden;

            // 3.3: Only rebuild the composite key string when inputs actually change
            if (trackName != _lastTrackName || carName != _lastCarName)
            {
                _lastTrackName    = trackName;
                _lastCarName      = carName;
                _lastTrackCarKey  = $"{trackName}_{carName}";

                if (Settings.TrackRecords.TryGetValue(trackName, out var trackDict) && trackDict.TryGetValue(carName, out var bestRecord))
                {
                    pluginManager.SetPropertyValue("CurrentCarBestLap",     this.GetType(), bestRecord.LapTime.ToString(@"mm\:ss\.fff"));
                    pluginManager.SetPropertyValue("CurrentCarBestLapTime", this.GetType(), bestRecord.LapTime.TotalMilliseconds);
                }
                else
                {
                    pluginManager.SetPropertyValue("CurrentCarBestLap",     this.GetType(), "-");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTime", this.GetType(), 0.0);
                }
            }

            // Detect new lap completion
            if (data.OldData != null && data.NewData.CurrentLap > data.OldData.CurrentLap)
            {
                bool     lapValid    = !data.NewData.LapInvalidated;
                TimeSpan lastLapTime = data.NewData.LastLapTime;

                // 3.1: Use log4net (bundled with SimHub) — buffered and thread-safe
                Log.Info($"[LapRecord] Lap finished. Valid={lapValid}, Time={lastLapTime.TotalSeconds:F3}s, Track={trackName}, Car={carName}");

                if (lapValid && lastLapTime > TimeSpan.Zero)
                {
                    bool isPb = true;
                    if (Settings.TrackRecords.TryGetValue(trackName, out var records) && records.TryGetValue(carName, out var existingRecord))
                    {
                        if (lastLapTime >= existingRecord.LapTime)
                            isPb = false;
                    }

                    if (isPb)
                    {
                        Log.Info($"[LapRecord] New PB: {lastLapTime:mm\\:ss\\.fff}");

                        if (!Settings.TrackRecords.ContainsKey(trackName))
                            Settings.TrackRecords[trackName] = new System.Collections.Generic.Dictionary<string, LapRecord>();

                        // 1.2: Use data.NewData.RoadTemperature directly instead of GetPropertyValue
                        double roadTemp   = data.NewData.RoadTemperature;
                        var    unitObj    = pluginManager.GetPropertyValue("DataCorePlugin.GameData.TemperatureUnit");
                        string unitMode   = unitObj != null ? unitObj.ToString().Replace("°", "").Trim() : "C";
                        if (unitMode.Length > 1) unitMode = unitMode.Substring(0, 1).ToUpper();
                        string trackTempStr = $"{roadTemp:F1}°{unitMode}";

                        string gameName = data.GameName ?? "Unknown";
                        if (!Settings.GameTyreOverrides.TryGetValue(gameName, out var gameOverride))
                            gameOverride = new GameTyreOverride();

                        string GetTyreVal(string overrideProp, string defaultProp)
                        {
                            if (!string.IsNullOrWhiteSpace(overrideProp))
                            {
                                var val = pluginManager.GetPropertyValue(overrideProp)?.ToString();
                                if (!string.IsNullOrWhiteSpace(val)) return val;
                            }
                            return pluginManager.GetPropertyValue(defaultProp)?.ToString() ?? "Unknown";
                        }

                        string fl = GetTyreVal(gameOverride.OverrideFL, "DataCorePlugin.GameData.TyreCompoundFrontLeft");
                        string fr = GetTyreVal(gameOverride.OverrideFR, "DataCorePlugin.GameData.TyreCompoundFrontRight");
                        string rl = GetTyreVal(gameOverride.OverrideRL, "DataCorePlugin.GameData.TyreCompoundRearLeft");
                        string rr = GetTyreVal(gameOverride.OverrideRR, "DataCorePlugin.GameData.TyreCompoundRearRight");

                        string tyreCompStr = (fl == fr && fl == rl && fl == rr && fl != "Unknown")
                            ? fl
                            : $"{fl}/{fr}/{rl}/{rr}";

                        var    wetnessObj    = pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Data.mAvgPathWetness");
                        string trackStateStr = "Unknown";
                        if (wetnessObj != null && double.TryParse(wetnessObj.ToString(), out double wetness))
                            trackStateStr = wetness == 0 ? "Dry" : (wetness < 0.3 ? "Intermediate" : "Wet");

                        Settings.TrackRecords[trackName][carName] = new LapRecord
                        {
                            GameName         = gameName,
                            CarName          = carName,
                            CarClass         = data.NewData.CarClass         ?? "Unknown",
                            Session          = data.NewData.SessionTypeName  ?? "Unknown",
                            LapTime          = lastLapTime,
                            FuelLevel        = data.NewData.Fuel > 0 ? data.NewData.Fuel : 0.0,
                            FuelUnit         = pluginManager.GetPropertyValue("DataCorePlugin.GameData.FuelUnit")?.ToString() ?? "L",
                            TyreCompound     = tyreCompStr,
                            TyreCompoundFL   = fl,
                            TyreCompoundFR   = fr,
                            TyreCompoundRL   = rl,
                            TyreCompoundRR   = rr,
                            TrackTemperature = trackTempStr,
                            TrackState       = trackStateStr,
                            RecordDate       = DateTime.Now
                        };

                        SaveSettings();

                        // Invalidate key cache so exported properties refresh next frame
                        _lastTrackName = null;
                        _lastCarName   = null;
                    }
                }
            }
        }
    }
}
