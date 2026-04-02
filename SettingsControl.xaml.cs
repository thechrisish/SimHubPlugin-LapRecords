using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SimHubLapRecordPlugin.Models;

namespace SimHubLapRecordPlugin
{
    public partial class SettingsControl : UserControl
    {
        public LapRecordPlugin Plugin { get; }
        private bool _isUpdatingFilter = false;
        // 3.4: Cache the tyre property list — GetAllPropertiesNames() is expensive and the list is stable at runtime
        private List<string> _cachedTyreProperties = null;

        public SettingsControl(LapRecordPlugin plugin)
        {
            Plugin = plugin;
            InitializeComponent();
            this.DataContext = this;
            LoadData();
            LoadColumnVisibilities();
            LoadTyreProperties();
        }

        private void LoadData()
        {
            if (Plugin?.Settings == null) return;

            var currentTrackObj = Plugin.PluginManager.GetPropertyValue("DataCorePlugin.GameData.TrackName");
            string currentTrack = currentTrackObj?.ToString() ?? "";

            string currentGame = Plugin.PluginManager.GameName ?? "";
            
            var distinctGames = Plugin.Settings.TrackRecords.Values
                .SelectMany(carDict => carDict.Values)
                .Select(r => r.GameName)
                .Where(g => !string.IsNullOrEmpty(g))
                .Distinct()
                .OrderBy(g => g)
                .ToList();
            distinctGames.Insert(0, "All Games");

            string selectedGame = GameFilterCombo.SelectedItem as string ?? "All Games";

            _isUpdatingFilter = true;
            if (AutoFilterCheckbox.IsChecked == true && !string.IsNullOrEmpty(currentGame))
            {
                selectedGame = currentGame;
                if (!distinctGames.Contains(currentGame)) distinctGames.Add(currentGame);
                GameFilterCombo.ItemsSource = distinctGames;
                GameFilterCombo.SelectedItem = currentGame;
                GameFilterCombo.IsEnabled = false;
            }
            else
            {
                GameFilterCombo.IsEnabled = true;
                GameFilterCombo.ItemsSource = distinctGames;
                GameFilterCombo.SelectedItem = distinctGames.Contains(selectedGame) ? selectedGame : "All Games";
            }
            _isUpdatingFilter = false;

            // Bind Lap Records
            var trackViewModels = Plugin.Settings.TrackRecords.Select(tr => new TrackViewModel
            {
                TrackName = tr.Key,
                Records = tr.Value.Values
                    .Where(r => selectedGame == "All Games" || string.Equals(r.GameName, selectedGame, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => x.LapTime).ToList()
            })
            .Where(tvm => tvm.Records.Count > 0)
            .OrderByDescending(x => string.Equals(x.TrackName, currentTrack, StringComparison.OrdinalIgnoreCase))
            .ThenBy(x => x.TrackName)
            .ToList();

            TrackItemsControl.ItemsSource = trackViewModels;

            // Bind Car Overrides
            var overridesList = Plugin.Settings.CarNameOverrides.Select(kvp => new OverrideViewModel
            {
                OriginalCarName  = kvp.Key,
                OverriddenName = kvp.Value
            }).OrderBy(x => x.OriginalCarName).ToList();
            
            OverridesGrid.ItemsSource = null;
            OverridesGrid.ItemsSource = overridesList;

            // Populate Car dropdown
            var distinctCars = Plugin.Settings.TrackRecords.Values
                .SelectMany(carDict => carDict.Keys)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            NewOriginalCarNameCombo.ItemsSource = distinctCars;

            // Bind Track Overrides
            var trackOverridesList = Plugin.Settings.TrackNameOverrides.Select(kvp => new TrackOverrideViewModel
            {
                OriginalTrackName = kvp.Key,
                UnifiedName       = kvp.Value
            }).OrderBy(x => x.OriginalTrackName).ToList();

            TrackOverridesGrid.ItemsSource = null;
            TrackOverridesGrid.ItemsSource = trackOverridesList;

            // Populate Track dropdown (all known raw track names)
            var distinctTracks = Plugin.Settings.TrackRecords.Keys
                .Concat(Plugin.Settings.TrackNameOverrides.Keys)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            NewOriginalTrackNameCombo.ItemsSource = distinctTracks;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void GameFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isUpdatingFilter) LoadData();
        }

        private void AutoFilterCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void AddOverride_Click(object sender, RoutedEventArgs e)
        {
            var original = NewOriginalCarNameCombo.Text?.Trim();
            var overriden = NewOverrideCarName.Text?.Trim();

            if (!string.IsNullOrEmpty(original) && !string.IsNullOrEmpty(overriden))
            {
                Plugin.Settings.CarNameOverrides[original] = overriden;
                
                // Retroactively apply the override to existing records, stamping OriginalCarName first
                foreach (var track in Plugin.Settings.TrackRecords.Values)
                {
                    if (track.ContainsKey(original))
                    {
                        var record = track[original];
                        // Stamp OriginalCarName if not already set (migration for pre-existing records)
                        if (string.IsNullOrEmpty(record.OriginalCarName))
                            record.OriginalCarName = original;
                        record.CarName = overriden;
                        track.Remove(original);
                        track[overriden] = record;
                    }
                }

                Plugin.SaveSettings();
                LoadData();
                NewOriginalCarNameCombo.Text = "";
                NewOverrideCarName.Text = "";
            }
        }

        private void RemoveOverride_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OverrideViewModel model)
            {
                if (Plugin.Settings.CarNameOverrides.ContainsKey(model.OriginalCarName))
                {
                    var overriden = Plugin.Settings.CarNameOverrides[model.OriginalCarName];

                    // Only revert records whose OriginalCarName matches this override's raw name.
                    // This is safe even if multiple cars were merged into the same override name.
                    foreach (var track in Plugin.Settings.TrackRecords.Values.ToList())
                    {
                        // Find all entries stored under the unified (overridden) name that originally
                        // belonged to this specific car.
                        var toRevert = track
                            .Where(kvp => string.Equals(kvp.Key, overriden, StringComparison.Ordinal)
                                       && string.Equals(kvp.Value.OriginalCarName, model.OriginalCarName, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        foreach (var kvp in toRevert)
                        {
                            track.Remove(kvp.Key);
                            var record = kvp.Value;
                            record.CarName = model.OriginalCarName;
                            track[model.OriginalCarName] = record;
                        }
                    }

                    Plugin.Settings.CarNameOverrides.Remove(model.OriginalCarName);
                    Plugin.SaveSettings();
                    LoadData();
                }
            }
        }
        
        private void OverridesGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.DataContext is OverrideViewModel model)
            {
                // Defer action slightly to allow framework to update model value
                Dispatcher.BeginInvoke(new Action(() => {
                    Plugin.Settings.CarNameOverrides[model.OriginalCarName] = model.OverriddenName;
                    Plugin.SaveSettings();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void LapRecordsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.DataContext is LapRecord record)
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    Plugin.SaveSettings();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void RemoveLapRecord_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LapRecord record)
            {
                foreach (var track in Plugin.Settings.TrackRecords.ToList())
                {
                    var carRecords = track.Value;
                    var targetKey = carRecords.FirstOrDefault(x => x.Value == record).Key;
                    if (targetKey != null)
                    {
                        carRecords.Remove(targetKey);
                        
                        // Cleanup track node if it's completely empty
                        if (carRecords.Count == 0)
                        {
                            Plugin.Settings.TrackRecords.Remove(track.Key);
                        }
                        
                        Plugin.SaveSettings();
                        LoadData();
                        return;
                    }
                }
            }
        }

        private void LoadColumnVisibilities()
        {
            if (Plugin?.Settings?.ColumnVisibility == null) return;

            ColumnVisibilityPanel.Children.Clear();
            foreach (var kvp in Plugin.Settings.ColumnVisibility)
            {
                var cb = new CheckBox
                {
                    Content = kvp.Key,
                    IsChecked = kvp.Value,
                    Margin = new Thickness(5)
                };
                string keyCopy = kvp.Key;
                cb.Checked   += (s, e) => { Plugin.Settings.ColumnVisibility[keyCopy] = true;  Plugin.SaveSettings(); ApplyColumnVisibility(); };
                cb.Unchecked += (s, e) => { Plugin.Settings.ColumnVisibility[keyCopy] = false; Plugin.SaveSettings(); ApplyColumnVisibility(); };
                ColumnVisibilityPanel.Children.Add(cb);
            }
        }

        // 2.4: Walk the visual tree to apply column visibility without triggering a full data reload
        private void ReloadColumnVisibilities()
        {
            ApplyColumnVisibility();
        }

        private void ApplyColumnVisibility()
        {
            foreach (var grid in FindVisualChildren<DataGrid>(TrackItemsControl))
            {
                foreach (var col in grid.Columns)
                {
                    string header = col.Header?.ToString();
                    if (header != null && Plugin.Settings.ColumnVisibility.TryGetValue(header, out bool visible))
                        col.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(System.Windows.DependencyObject parent) where T : System.Windows.DependencyObject
        {
            if (parent == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match) yield return match;
                foreach (var nested in FindVisualChildren<T>(child))
                    yield return nested;
            }
        }

        private void LoadTyreProperties()
        {
            var options = new List<string>();
            try
            {
                // 3.4: Only query GetAllPropertiesNames() once; result is stable for the session
                if (_cachedTyreProperties == null)
                {
                    var allProps = Plugin.PluginManager.GetAllPropertiesNames();
                    _cachedTyreProperties = allProps != null
                        ? allProps.Where(p => p.IndexOf("tyre", StringComparison.OrdinalIgnoreCase) >= 0
                                           || p.IndexOf("tire", StringComparison.OrdinalIgnoreCase) >= 0).ToList()
                        : new List<string>();
                }
                options = _cachedTyreProperties;
            }
            catch { }

            OverrideFL.ItemsSource = options;
            OverrideFR.ItemsSource = options;
            OverrideRL.ItemsSource = options;
            OverrideRR.ItemsSource = options;

            var trackedGames = Plugin.Settings.TrackRecords.Values.SelectMany(c => c.Values).Select(r => r.GameName);
            var defaultGames = new[] { "AssettoCorsa", "AssettoCorsaCompetizione", "Automobilista", "Automobilista2", "BeamNg", "CodemastersDirtRally2", "EA_WRC", "F12022", "F123", "F124", "ForzaHorizon5", "ForzaMotorsport", "IRacing", "LeMansUltimate", "ProjectCars2", "RaceRoom", "RFactor2", "RichardBurnsRally" };
            
            var games = trackedGames.Concat(defaultGames).Where(g => !string.IsNullOrEmpty(g)).Distinct().OrderBy(x => x).ToList();
            GameOverrideCombo.ItemsSource = games;

            CompoundsListControl.ItemsSource = Plugin.Settings.TyreCompoundDefinitions;
            TyreCompoundConverter.ContextSettings = Plugin.Settings;
        }

        private void GameOverrideCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string game = GameOverrideCombo.SelectedItem as string ?? GameOverrideCombo.Text;
            if (!string.IsNullOrEmpty(game) && Plugin.Settings.GameTyreOverrides.TryGetValue(game, out var overrides))
            {
                OverrideFL.Text = overrides.OverrideFL;
                OverrideFR.Text = overrides.OverrideFR;
                OverrideRL.Text = overrides.OverrideRL;
                OverrideRR.Text = overrides.OverrideRR;
            }
            else
            {
                OverrideFL.Text = "";
                OverrideFR.Text = "";
                OverrideRL.Text = "";
                OverrideRR.Text = "";
            }
        }

        private void SaveOverrides_Click(object sender, RoutedEventArgs e)
        {
            string game = GameOverrideCombo.Text?.Trim();
            if (!string.IsNullOrEmpty(game))
            {
                Plugin.Settings.GameTyreOverrides[game] = new GameTyreOverride
                {
                    OverrideFL = OverrideFL.Text,
                    OverrideFR = OverrideFR.Text,
                    OverrideRL = OverrideRL.Text,
                    OverrideRR = OverrideRR.Text
                };
                Plugin.SaveSettings();
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && (tb.Text == "Name..." || tb.Text == "Abbr" || tb.Text == "Color"))
            {
                tb.Text = "";
            }
        }

        private void AddCompound_Click(object sender, RoutedEventArgs e)
        {
            string name = NewCompoundName.Text?.Trim();
            string abbrev = NewCompoundAbbrev.Text?.Trim();
            string color = NewCompoundColor.Text?.Trim();

            if (!string.IsNullOrEmpty(name) && !Plugin.Settings.TyreCompoundDefinitions.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                Plugin.Settings.TyreCompoundDefinitions.Add(new TyreCompoundDef 
                { 
                    Name = name, 
                    Abbreviation = string.IsNullOrEmpty(abbrev) ? name.Substring(0, 1) : abbrev, 
                    BackgroundColor = string.IsNullOrEmpty(color) ? "Gray" : color 
                });
                Plugin.SaveSettings();
                NewCompoundName.Text  = "Name...";
                NewCompoundAbbrev.Text = "Abbr";
                NewCompoundColor.Text  = "Color";
                // 2.2: ObservableCollection notifies the UI automatically — no manual ItemsSource reset needed
            }
        }

        private void RemoveCompound_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TyreCompoundDef compound)
            {
                Plugin.Settings.TyreCompoundDefinitions.Remove(compound);
                Plugin.SaveSettings();
                // 2.2: ObservableCollection notifies the UI automatically — no manual ItemsSource reset needed
            }
        }


        private void TyreEditToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.ToggleButton toggle)
            {
                var panel = toggle.Parent as StackPanel;
                if (panel != null)
                {
                    var popup = panel.Children.OfType<System.Windows.Controls.Primitives.Popup>().FirstOrDefault();
                    if (popup != null)
                    {
                        // 2.1: Use Tag as a one-time subscription sentinel to prevent handler accumulation
                        if (popup.Tag == null)
                        {
                            popup.Closed += (s, ev) => toggle.IsChecked = false;
                            popup.Tag = true;
                        }
                        popup.IsOpen = toggle.IsChecked == true;
                    }
                }
            }
        }

        private void TyreTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Plugin.SaveSettings();
        }

        // 2.5: TyrePopupCombo_SelectionChanged removed — was dead code after ComboBoxes replaced with TextBoxes

        private void CompoundEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            Plugin.SaveSettings();
        }

        private void BackupRecords_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Files|*.json",
                FileName = $"SimHub_LapRecordPlugin_Backup_{DateTime.Now:yyyyMMdd_HHmm}.json"
            };

            if (dialog.ShowDialog() == true)
            {
                // Serialize the full Settings object so overrides, compounds and tyre mappings
                // are all included alongside the lap records.
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(Plugin.Settings, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(dialog.FileName, json);
                MessageBox.Show("All settings backed up successfully.", "Backup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RestoreRecords_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                if (MessageBox.Show("Restoring this backup will completely overwrite all current settings and lap records. Are you sure?",
                    "Restore Backup", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var json = System.IO.File.ReadAllText(dialog.FileName);

                        // Try new-style backup first (full Settings object)
                        Settings restored = null;
                        try
                        {
                            restored = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(json);
                            // Sanity check: a records-only file will deserialize as Settings but
                            // TrackRecords will be null because the root is a dictionary, not an object.
                            if (restored?.TrackRecords == null) restored = null;
                        }
                        catch { restored = null; }

                        if (restored != null)
                        {
                            // New-style: replace the entire Settings object
                            Plugin.Settings.TrackRecords         = restored.TrackRecords         ?? Plugin.Settings.TrackRecords;
                            Plugin.Settings.CarNameOverrides     = restored.CarNameOverrides     ?? Plugin.Settings.CarNameOverrides;
                            Plugin.Settings.TrackNameOverrides   = restored.TrackNameOverrides   ?? Plugin.Settings.TrackNameOverrides;
                            Plugin.Settings.GameTyreOverrides    = restored.GameTyreOverrides    ?? Plugin.Settings.GameTyreOverrides;
                            Plugin.Settings.TyreCompoundDefinitions = restored.TyreCompoundDefinitions ?? Plugin.Settings.TyreCompoundDefinitions;
                            Plugin.Settings.ColumnVisibility     = restored.ColumnVisibility     ?? Plugin.Settings.ColumnVisibility;
                        }
                        else
                        {
                            // Legacy-style: records-only dictionary
                            var records = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, SimHubLapRecordPlugin.Models.LapRecord>>>(json);
                            if (records != null)
                                Plugin.Settings.TrackRecords = records;
                            else
                                throw new InvalidOperationException("File does not contain recognisable backup data.");
                        }

                        Plugin.SaveSettings();
                        LoadData();
                        MessageBox.Show("Settings successfully restored.", "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to restore settings. The file may be invalid or corrupt.\n\n" + ex.Message,
                            "Restore Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // ── Track Overrides ──────────────────────────────────────────────────────────

        private void AddTrackOverride_Click(object sender, RoutedEventArgs e)
        {
            var original = NewOriginalTrackNameCombo.Text?.Trim();
            var unified  = NewUnifiedTrackName.Text?.Trim();

            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(unified)) return;

            Plugin.Settings.TrackNameOverrides[original] = unified;

            // Ensure the unified bucket exists
            if (!Plugin.Settings.TrackRecords.ContainsKey(unified))
                Plugin.Settings.TrackRecords[unified] = new Dictionary<string, Models.LapRecord>();

            // Move all records from the raw bucket into the unified bucket
            if (Plugin.Settings.TrackRecords.TryGetValue(original, out var sourceBucket))
            {
                var destBucket = Plugin.Settings.TrackRecords[unified];
                foreach (var kvp in sourceBucket)
                {
                    var record = kvp.Value;
                    // Stamp OriginalTrackName if not already set (migration for pre-existing records)
                    if (string.IsNullOrEmpty(record.OriginalTrackName))
                        record.OriginalTrackName = original;
                    // Use car name as key; last write wins if there's a collision
                    destBucket[kvp.Key] = record;
                }
                Plugin.Settings.TrackRecords.Remove(original);
            }

            Plugin.SaveSettings();
            LoadData();
            NewOriginalTrackNameCombo.Text = "";
            NewUnifiedTrackName.Text      = "";
        }

        private void RemoveTrackOverride_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.DataContext is TrackOverrideViewModel model)) return;
            if (!Plugin.Settings.TrackNameOverrides.ContainsKey(model.OriginalTrackName))   return;

            var unified = Plugin.Settings.TrackNameOverrides[model.OriginalTrackName];

            // Re-sort records in the unified bucket back to their original track buckets
            // (only those whose OriginalTrackName matches this override)
            if (Plugin.Settings.TrackRecords.TryGetValue(unified, out var unifiedBucket))
            {
                var toMove = unifiedBucket
                    .Where(kvp => string.Equals(kvp.Value.OriginalTrackName, model.OriginalTrackName, StringComparison.OrdinalIgnoreCase)
                               || string.IsNullOrEmpty(kvp.Value.OriginalTrackName))
                    .ToList();

                foreach (var kvp in toMove)
                {
                    var record    = kvp.Value;
                    var targetKey = string.IsNullOrEmpty(record.OriginalTrackName) ? model.OriginalTrackName : record.OriginalTrackName;

                    if (!Plugin.Settings.TrackRecords.ContainsKey(targetKey))
                        Plugin.Settings.TrackRecords[targetKey] = new Dictionary<string, Models.LapRecord>();

                    Plugin.Settings.TrackRecords[targetKey][kvp.Key] = record;
                    unifiedBucket.Remove(kvp.Key);
                }

                // Remove unified bucket if now empty
                if (unifiedBucket.Count == 0)
                    Plugin.Settings.TrackRecords.Remove(unified);
            }

            Plugin.Settings.TrackNameOverrides.Remove(model.OriginalTrackName);
            Plugin.SaveSettings();
            LoadData();
        }

        private void TrackOverridesGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.DataContext is TrackOverrideViewModel model)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Plugin.Settings.TrackNameOverrides[model.OriginalTrackName] = model.UnifiedName;
                    Plugin.SaveSettings();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void LapRecordsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid grid && Plugin?.Settings?.ColumnVisibility != null)
            {
                foreach (var col in grid.Columns)
                {
                    string headerStr = col.Header?.ToString();
                    if (headerStr != null && Plugin.Settings.ColumnVisibility.TryGetValue(headerStr, out bool isVisible))
                    {
                        col.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }
    }

    public class TrackViewModel
    {
        public string TrackName { get; set; }
        public List<LapRecord> Records { get; set; }
    }

    public class OverrideViewModel
    {
        public string OriginalCarName { get; set; }
        public string OverriddenName { get; set; }
    }

    public class TrackOverrideViewModel
    {
        public string OriginalTrackName { get; set; }
        public string UnifiedName { get; set; }
    }

    [System.Windows.Data.ValueConversion(typeof(string), typeof(object))]
    public class TyreCompoundConverter : System.Windows.Data.IValueConverter
    {
        public static Settings ContextSettings { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string compoundName = value as string;
            string returnType = parameter as string; // "Color" or "Text"
            var def = ContextSettings?.TyreCompoundDefinitions?.FirstOrDefault(x => string.Equals(x.Name, compoundName, StringComparison.OrdinalIgnoreCase));
            
            if (def == null) 
            {
                if (returnType == "Color") return "DarkGray";
                return string.IsNullOrEmpty(compoundName) ? "?" : compoundName.Substring(0, 1).ToUpper();
            }

            if (returnType == "Color") return def.BackgroundColor;
            return def.Abbreviation;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // User typed an abbreviation (e.g. "M") — resolve to full compound name
            string input = value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(input)) return System.Windows.DependencyProperty.UnsetValue;

            var compounds = ContextSettings?.TyreCompoundDefinitions;
            if (compounds == null) return input;

            // Match abbreviation first, then fall back to full name match
            var match = compounds.FirstOrDefault(c => string.Equals(c.Abbreviation, input, StringComparison.OrdinalIgnoreCase))
                     ?? compounds.FirstOrDefault(c => string.Equals(c.Name, input, StringComparison.OrdinalIgnoreCase));

            return match?.Name ?? input;
        }
    }

    public class BindingProxy : System.Windows.Freezable
    {
        protected override System.Windows.Freezable CreateInstanceCore() => new BindingProxy();

        public static readonly System.Windows.DependencyProperty DataProperty =
            System.Windows.DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy));

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
    }
}
