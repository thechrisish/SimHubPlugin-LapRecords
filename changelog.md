# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

Changelog is written by Claude Sonnet and Gemini Pro.

## [1.0.1] - 2026-03-29
### Added
- Expanded SimHub property endpoints extensively. The plugin now natively exposes `TrackState`, `TrackTemp`, `TyreFL`, `TyreFR`, `TyreRL`, `TyreRR`, and `Fuel` properties dynamically bound into the `CurrentCarBestLap` dataset.
- Added a brand new, fully populated `CurrentClassBestLap` property tree replicating the entire parameter set. It dynamically scans and tracks the absolute best lap time across all cars specifically matching your current car's Class designation locally.

### Changed
- Scaled down the plugin's stopwatch icon by enclosing the geometry inside a transparent bounding box, ensuring visual parity with native SimHub side-menu icons.
- Modified data injection and mapping loops across `Class`, `Session`, `Fuel Level`, `Track Temp`, and `Track State` columns to explicitly output correctly empty strings instead of default "Unknown" or zero placeholders when a game sim fails to provide targeted telemetry correctly.
- Created robust legacy JSON data conversions safely translating old string flags back over to blank spaces locally upon data grid loading gracefully resolving data conflicts.

## [1.0.0] - 2026-03-28
### Added
- Added a new `Settings` tab allowing column visibility customization and global tyre extraction parameters.
- Implemented a dedicated section inside the Settings tab to add, remove, and manage custom tyre compounds. Users can now assign custom colors and acronym abbreviations to each compound locally and alter preexisting entries seamlessly.
- Extended tyre data extraction, natively splitting and tracking FL, FR, RL, RR into 4 distinct quadrants upon crossing the lapline.
- Added a `✎` edit button to each row in the Lap Records tyre column. Clicking it opens a dark flyout popup showing four per-corner ComboBox selectors (FL, FR, RL, RR) populated from the custom tyre compound definitions in Settings. Changes are saved immediately on selection.
- Engineered a brand new Database Management module containing 2 primary actions: `Backup Lap Records` and `Restore Lap Records`, leveraging native graphical file dialogs to export your records completely to disk explicitly avoiding accidental overwrites.

### Changed
- Converted global tyre overrides into a strictly per-game mapping architecture that accurately falls back to native data when unavailable.
- Replaced the textual Tyre identifier inside the main records datagrid with an automatically colored 4-square abbreviation grid visually mapping out mixed tyre setups.
- Redesigned the primary UI column for Tyre identifiers explicitly setting it back to ReadOnly, removing the dropdown mapping overlay from recorded tables.
- Reordered the `Lap Records` datagrid columns to permanently lock `Laptime` to the right-most position.
- Refactored the core Plugin SVG icon switching it entirely to a minimalist white stopwatch instead of the generic refresh ring.

### Fixed
- Fixed an issue where the new tyre object schema caused Newtonsoft.Json to crash during deserialization of older string configurations, which would cause the plugin to generate a brand new settings file and drop all legacy track lap records. Old properties are now safely swallowed and preserved.
- Corrected a severe DataGrid binding limitation silently failing compound extraction mappings for custom textual strings like "Unknown", which resulted in empty tyre selections rendering inside the UI editor.
- Blocked empty placeholder spacer rows from visually rendering inside the main laptime tracking table.
- Disconnected the UI Default Tyres mapping mechanism from automatically appending inside JSON configurations directly, entirely fixing the bug mapping default sets redundantly across plugin re-initialization boots.

## [0.0.8] - 2026-03-26
### Added
- Created a top-level filtering layout resting above the Lap Records data table natively allowing records to be strictly filtered by `GameName` instances.
- Added a new Auto-Filter toggle gracefully locking the data table specifically to whichever game engine SimHub natively detects on connection.
- A custom vector-based white Stopwatch icon has been injected directly into the WPF configuration cleanly establishing the visual aesthetics against other SimHub core modules.

### Changed
- Reworked the `CELSIUS` parsing strings aggressively stripping all string artifacts beyond the core "C" and "F" formatters natively resolving legacy game integration errors.

## [0.0.7] - 2026-03-26
### Fixed
- Re-coded the Track Temperature format rendering. It now actively strips arbitrary lowercase degrees and explicitly builds proper, capitalized bounds depending purely on your active SimHub parameter settings (rendering strictly as `°C` or `°F`).

## [0.0.6] - 2026-03-26
### Added
- Inserted a brand new `Date` column mapping directly onto the native Lap Records table showcasing exactly when the record was achieved.
- Added a `Remove` action button directly onto every row inside the Lap Records table, cleanly bypassing the need to edit the raw `JSON` configuration file to clear out bad laps.

## [0.0.5] - 2026-03-26
### Added
- Added a new bindable `DataGridComboBoxColumn` for "Track State" to directly support "Unknown", "Dry", "Intermediate", and "Wet" manual interventions inside the Lap Records data table.
### Changed
- Removing a "Car Name Override" entry will now actively reverse and restore all affected records back to their original game-native tracking names natively.
- Adjusted telemetry parsing for path wetness to trigger "Intermediate" bounds instead of the previous "Damp" mapping.

## [0.0.4] - 2026-03-26
### Changed
- Adding a new Car Name Override now natively retroactively applies to all previously recorded laps stored under the original game name in the main table. 
- The Lap Records settings grid now dynamically pins the track you are currently driving on to the very top of the list for immediate access.

## [0.0.3] - 2026-03-26
### Added
- Replaced the generic Textboxes in the Settings UI with interactive, prepopulated ComboBoxes for both Track Tyres and Original Game Car Names.
- Tyre compounds can now be directly altered via dropdown list inside the Lap Records data table.
### Changed
- Refined track temperature fetches to explicitly check `DataCorePlugin.GameData.RoadTemperature`.
- Integrated PC2/AMS2 `mAvgPathWetness` percentage tracking to establish true Track States internally.
- Disabled direct compound index mapping algorithms in favor of manual verification/UI correction overrides.

## [0.0.2] - 2026-03-26
### Fixed
- Improved lap time validation reliability for games where `LapInvalidated` behaves inconsistently.
- Fixed an issue where absent track state or tyre compound data could produce error states.
### Added
- Added a "Refresh Data" button to the Settings UI to allow updating the list manually.
- Added detailed telemetry logging to `SimHub.log` for lap completion tracking.

## [0.0.1] - 2026-03-26
### Added
- Initial creation of the SimHub Lap Record Plugin.
- Automatic logging of best lap times grouped by track and car.
- Support for capturing Class, Session, Laptime, Tyre Compound, Track Temperature, and Track State.
- Exposed `CurrentCarBestLap` and `CurrentCarBestLapTime` properties to SimHub.
- WPF Settings UI to visualize recorded laps.
- Configurable dictionary in UI to overwrite/remap game-default car names to friendly display names.
