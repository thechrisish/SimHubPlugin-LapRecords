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
    [PluginAuthor("theCHRISish")]
    [PluginName("Lap Record Plugin")]
    public class LapRecordPlugin : IPlugin, IDataPlugin, IWPFSettingsV2
    {
        public PluginManager PluginManager { get; set; }
        public Settings Settings;

        /// <summary>Raised on the game thread when a new personal best is saved.</summary>
        public event EventHandler LapRecordUpdated;

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(LapRecordPlugin));

        // Per-frame caching to avoid redundant string allocations (3.3)
        private string _lastTrackName = null;
        private string _lastCarName = null;
        private string _lastGameName = null;
        private string _lastCarClass = null;

        public string LeftMenuTitle => "Lap Records";

        public ImageSource PictureIcon
        {
            get
            {
                var geometry = Geometry.Parse("M15,1H9V3H15V1M11,14H13V8H11V14M19.03,7.39L20.45,5.97C20,5.46 19.55,5 19.04,4.56L17.62,5.98C16.07,4.74 14.12,4 12,4C7.03,4 3,8.03 3,13C3,17.97 7.03,22 12,22C16.97,22 21,17.97 21,13C21,10.88 20.26,8.93 19.03,7.39M12,20C8.13,20 5,16.87 5,13C5,9.13 8.13,6 12,6C15.87,6 19,9.13 19,13C19,16.87 15.87,20 12,20Z");
                var group = new DrawingGroup();
                group.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new System.Windows.Rect(-3, -3, 30, 30))));
                group.Children.Add(new GeometryDrawing(Brushes.White, new Pen(Brushes.White, 0), geometry));
                return new DrawingImage(group);
            }
        }

        public void Init(PluginManager pluginManager)
        {
            // 1.1: Use SimHub's native settings persistence (IPlugin extension method)
            Settings = this.ReadCommonSettings<Settings>("Settings", () => new Settings());

            if (Settings.TyreCompoundDefinitions == null)
                Settings.TyreCompoundDefinitions = new System.Collections.ObjectModel.ObservableCollection<TyreCompoundDef>();

            // Default compound catalogue — new entries added here are auto-merged on boot
            var defaultCompounds = new[]
            {
                new TyreCompoundDef { Name = "Soft",          Abbreviation = "S",   BackgroundColor = "White"       },
                new TyreCompoundDef { Name = "Medium",        Abbreviation = "M",   BackgroundColor = "Yellow"      },
                new TyreCompoundDef { Name = "Hard",          Abbreviation = "H",   BackgroundColor = "Red"         },
                new TyreCompoundDef { Name = "Intermediate",  Abbreviation = "I",   BackgroundColor = "Green"       },
                new TyreCompoundDef { Name = "Wet",           Abbreviation = "W",   BackgroundColor = "Blue"        },
                new TyreCompoundDef { Name = "Speedway",      Abbreviation = "SPD", BackgroundColor = "Orange"      },
                new TyreCompoundDef { Name = "Primary",       Abbreviation = "PRI", BackgroundColor = "LightGray"   },
                new TyreCompoundDef { Name = "Alternate",     Abbreviation = "ALT", BackgroundColor = "Khaki"       },
                new TyreCompoundDef { Name = "Extreme Wet",   Abbreviation = "XW",  BackgroundColor = "DarkBlue"    },
                new TyreCompoundDef { Name = "Vintage",       Abbreviation = "VIN", BackgroundColor = "SaddleBrown" },
                new TyreCompoundDef { Name = "Street",        Abbreviation = "STR", BackgroundColor = "DimGray"     },
                new TyreCompoundDef { Name = "All-Weather",   Abbreviation = "AW",  BackgroundColor = "Teal"        },
                new TyreCompoundDef { Name = "Semi-Slick",    Abbreviation = "SMS", BackgroundColor = "Purple"      },
                new TyreCompoundDef { Name = "Dry",           Abbreviation = "DRY", BackgroundColor = "Tan"         },
                new TyreCompoundDef { Name = "Gravel Soft",   Abbreviation = "GrS", BackgroundColor = "#C4A882"     },
                new TyreCompoundDef { Name = "Gravel Medium", Abbreviation = "GrM", BackgroundColor = "#A0875A"     },
                new TyreCompoundDef { Name = "Gravel Hard",   Abbreviation = "GrH", BackgroundColor = "#7D6840"     },
                new TyreCompoundDef { Name = "Tarmac Soft",   Abbreviation = "TaS", BackgroundColor = "#D0D0D0"     },
                new TyreCompoundDef { Name = "Tarmac Medium", Abbreviation = "TaM", BackgroundColor = "#A0A0A0"     },
                new TyreCompoundDef { Name = "Tarmac Hard",   Abbreviation = "TaH", BackgroundColor = "#707070"     },
                new TyreCompoundDef { Name = "Snow",          Abbreviation = "SNW", BackgroundColor = "AliceBlue"   },
                new TyreCompoundDef { Name = "Winter",        Abbreviation = "WNT", BackgroundColor = "LightCyan"   }
            };

            // Auto-merge: append any default compounds not already present (matched by Name)
            foreach (var def in defaultCompounds)
            {
                if (!Settings.TyreCompoundDefinitions.Any(c => string.Equals(c.Name, def.Name, StringComparison.OrdinalIgnoreCase)))
                    Settings.TyreCompoundDefinitions.Add(def);
            }

            pluginManager.AddProperty("CurrentCarBestLap", this.GetType(), "-");
            pluginManager.AddProperty("CurrentCarBestLapTime", this.GetType(), 0.0);
            pluginManager.AddProperty("CurrentCarBestLapTrackState", this.GetType(), "");
            pluginManager.AddProperty("CurrentCarBestLapTrackTemp", this.GetType(), "");
            pluginManager.AddProperty("CurrentCarBestLapTyreFL", this.GetType(), "");
            pluginManager.AddProperty("CurrentCarBestLapTyreFR", this.GetType(), "");
            pluginManager.AddProperty("CurrentCarBestLapTyreRL", this.GetType(), "");
            pluginManager.AddProperty("CurrentCarBestLapTyreRR", this.GetType(), "");
            pluginManager.AddProperty("CurrentCarBestLapFuel", this.GetType(), "");

            pluginManager.AddProperty("CurrentClassBestLap", this.GetType(), "-");
            pluginManager.AddProperty("CurrentClassBestLapTime", this.GetType(), 0.0);
            pluginManager.AddProperty("CurrentClassBestLapTrackState", this.GetType(), "");
            pluginManager.AddProperty("CurrentClassBestLapTrackTemp", this.GetType(), "");
            pluginManager.AddProperty("CurrentClassBestLapTyreFL", this.GetType(), "");
            pluginManager.AddProperty("CurrentClassBestLapTyreFR", this.GetType(), "");
            pluginManager.AddProperty("CurrentClassBestLapTyreRL", this.GetType(), "");
            pluginManager.AddProperty("CurrentClassBestLapTyreRR", this.GetType(), "");
            pluginManager.AddProperty("CurrentClassBestLapFuel", this.GetType(), "");
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

            string trackName = data.NewData.TrackNameWithConfig ?? data.NewData.TrackName ?? "UnknownTrack";
            string carName   = data.NewData.CarModel  ?? "UnknownCar";
            string gameName  = data.GameName ?? "Unknown";
            string carClass  = !string.IsNullOrWhiteSpace(data.NewData.CarClass) ? data.NewData.CarClass : "";

            string rawCarName = carName;
            if (Settings.CarNameOverrides.TryGetValue(carName, out var overridden) && !string.IsNullOrWhiteSpace(overridden))
                carName = overridden;

            // Apply track name override (merge multiple sim track strings to one unified key)
            string rawTrackName = trackName;
            if (Settings.TrackNameOverrides.TryGetValue(trackName, out var overriddenTrack) && !string.IsNullOrWhiteSpace(overriddenTrack))
                trackName = overriddenTrack;

            if (trackName != _lastTrackName || carName != _lastCarName || gameName != _lastGameName || carClass != _lastCarClass)
            {
                _lastTrackName = trackName;
                _lastCarName   = carName;
                _lastGameName  = gameName;
                _lastCarClass  = carClass;

                if (Settings.TrackRecords.TryGetValue(trackName, out var trackDict) && trackDict.TryGetValue(carName, out var bestRecord))
                {
                    pluginManager.SetPropertyValue("CurrentCarBestLap",           this.GetType(), bestRecord.LapTime.ToString(@"mm\:ss\.fff"));
                    pluginManager.SetPropertyValue("CurrentCarBestLapTime",       this.GetType(), bestRecord.LapTime.TotalMilliseconds);
                    pluginManager.SetPropertyValue("CurrentCarBestLapTrackState", this.GetType(), bestRecord.TrackState ?? "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTrackTemp",  this.GetType(), bestRecord.TrackTemperature ?? "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTyreFL",     this.GetType(), bestRecord.TyreCompoundFL ?? "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTyreFR",     this.GetType(), bestRecord.TyreCompoundFR ?? "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTyreRL",     this.GetType(), bestRecord.TyreCompoundRL ?? "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTyreRR",     this.GetType(), bestRecord.TyreCompoundRR ?? "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapFuel",       this.GetType(), bestRecord.DisplayFuel ?? "");
                }
                else
                {
                    pluginManager.SetPropertyValue("CurrentCarBestLap",           this.GetType(), "-");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTime",       this.GetType(), 0.0);
                    pluginManager.SetPropertyValue("CurrentCarBestLapTrackState", this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTrackTemp",  this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTyreFL",     this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTyreFR",     this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTyreRL",     this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapTyreRR",     this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentCarBestLapFuel",       this.GetType(), "");
                }

                LapRecord bestClassRecord = null;
                if (Settings.TrackRecords.TryGetValue(trackName, out var records) && !string.IsNullOrWhiteSpace(carClass))
                {
                    bestClassRecord = records.Values
                        .Where(r => r.GameName == gameName && r.CarClass == carClass)
                        .OrderBy(r => r.LapTime)
                        .FirstOrDefault();
                }

                if (bestClassRecord != null)
                {
                    pluginManager.SetPropertyValue("CurrentClassBestLap",           this.GetType(), bestClassRecord.LapTime.ToString(@"mm\:ss\.fff"));
                    pluginManager.SetPropertyValue("CurrentClassBestLapTime",       this.GetType(), bestClassRecord.LapTime.TotalMilliseconds);
                    pluginManager.SetPropertyValue("CurrentClassBestLapTrackState", this.GetType(), bestClassRecord.TrackState ?? "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTrackTemp",  this.GetType(), bestClassRecord.TrackTemperature ?? "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTyreFL",     this.GetType(), bestClassRecord.TyreCompoundFL ?? "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTyreFR",     this.GetType(), bestClassRecord.TyreCompoundFR ?? "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTyreRL",     this.GetType(), bestClassRecord.TyreCompoundRL ?? "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTyreRR",     this.GetType(), bestClassRecord.TyreCompoundRR ?? "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapFuel",       this.GetType(), bestClassRecord.DisplayFuel ?? "");
                }
                else
                {
                    pluginManager.SetPropertyValue("CurrentClassBestLap",           this.GetType(), "-");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTime",       this.GetType(), 0.0);
                    pluginManager.SetPropertyValue("CurrentClassBestLapTrackState", this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTrackTemp",  this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTyreFL",     this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTyreFR",     this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTyreRL",     this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapTyreRR",     this.GetType(), "");
                    pluginManager.SetPropertyValue("CurrentClassBestLapFuel",       this.GetType(), "");
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
                        string trackTempStr = roadTemp != 0 ? $"{roadTemp:F1}°{unitMode}" : "";

                        if (!Settings.GameTyreOverrides.TryGetValue(gameName, out var gameOverride))
                            gameOverride = new GameTyreOverride();

                        // Resolve game-specific fallback tyre properties
                        string fallbackFL = null, fallbackFR = null, fallbackRL = null, fallbackRR = null;
                        if (gameName == "LeMansUltimate" || gameName == "LMU")
                        {
                            fallbackFL = "LMU_NeoRedPlugin.Tyre.FL_TyreCompound_Name";
                            fallbackFR = "LMU_NeoRedPlugin.Tyre.FR_TyreCompound_Name";
                            fallbackRL = "LMU_NeoRedPlugin.Tyre.RL_TyreCompound_Name";
                            fallbackRR = "LMU_NeoRedPlugin.Tyre.RR_TyreCompound_Name";
                        }
                        else if (gameName == "Automobilista2")
                        {
                            fallbackFL = "DataCorePlugin.GameRawData.mTyreCompound01.value";
                            fallbackFR = "DataCorePlugin.GameRawData.mTyreCompound02.value";
                            fallbackRL = "DataCorePlugin.GameRawData.mTyreCompound03.value";
                            fallbackRR = "DataCorePlugin.GameRawData.mTyreCompound04.value";
                        }

                        string GetTyreVal(string overrideProp, string defaultProp, string fallbackProp)
                        {
                            if (!string.IsNullOrWhiteSpace(overrideProp))
                            {
                                var val = pluginManager.GetPropertyValue(overrideProp)?.ToString();
                                if (!string.IsNullOrWhiteSpace(val)) return val;
                            }
                            if (!string.IsNullOrWhiteSpace(fallbackProp))
                            {
                                var val = pluginManager.GetPropertyValue(fallbackProp)?.ToString();
                                if (!string.IsNullOrWhiteSpace(val)) return val;
                            }
                            return pluginManager.GetPropertyValue(defaultProp)?.ToString() ?? "Unknown";
                        }

                        string fl = GetTyreVal(gameOverride.OverrideFL, "DataCorePlugin.GameData.TyreCompoundFrontLeft", fallbackFL);
                        string fr = GetTyreVal(gameOverride.OverrideFR, "DataCorePlugin.GameData.TyreCompoundFrontRight", fallbackFR);
                        string rl = GetTyreVal(gameOverride.OverrideRL, "DataCorePlugin.GameData.TyreCompoundRearLeft", fallbackRL);
                        string rr = GetTyreVal(gameOverride.OverrideRR, "DataCorePlugin.GameData.TyreCompoundRearRight", fallbackRR);

                        string tyreCompStr = (fl == fr && fl == rl && fl == rr && fl != "Unknown")
                            ? fl
                            : $"{fl}/{fr}/{rl}/{rr}";

                        var    wetnessObj    = pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Data.mAvgPathWetness");
                        string trackStateStr = "Unknown";
                        if (wetnessObj != null && double.TryParse(wetnessObj.ToString(), out double wetness))
                            trackStateStr = wetness == 0 ? "Dry" : (wetness < 0.3 ? "Intermediate" : "Wet");

                        Settings.TrackRecords[trackName][carName] = new LapRecord
                        {
                            GameName          = gameName,
                            CarName           = carName,
                            OriginalCarName   = rawCarName,
                            OriginalTrackName = rawTrackName,
                            CarClass          = !string.IsNullOrWhiteSpace(data.NewData.CarClass) ? data.NewData.CarClass : "",
                            Session           = !string.IsNullOrWhiteSpace(data.NewData.SessionTypeName) ? data.NewData.SessionTypeName : "",
                            LapTime           = lastLapTime,
                            FuelLevel         = data.NewData.Fuel > 0 ? data.NewData.Fuel : 0.0,
                            FuelUnit          = pluginManager.GetPropertyValue("DataCorePlugin.GameData.FuelUnit")?.ToString() ?? "L",
                            TyreCompound      = tyreCompStr,
                            TyreCompoundFL    = fl,
                            TyreCompoundFR    = fr,
                            TyreCompoundRL    = rl,
                            TyreCompoundRR    = rr,
                            TrackTemperature  = trackTempStr,
                            TrackState        = trackStateStr,
                            RecordDate        = DateTime.Now
                        };

                        SaveSettings();
                        LapRecordUpdated?.Invoke(this, EventArgs.Empty);

                        // Invalidate key cache so exported properties refresh next frame
                        _lastTrackName = null;
                        _lastCarName   = null;
                        _lastGameName  = null;
                        _lastCarClass  = null;
                    }
                }
            }
        }
    }
}
