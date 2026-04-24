# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

Changelog is written by Claude Sonnet and Gemini Pro.

## [1.2.2] - 2026-04-24
### Fixed
- Fixed lap completion detection incorrectly using `CompletedLaps` (which can increment for any car on track) instead of `CurrentLap` (player-only), causing the plugin to log lap data from other drivers in multiplayer sessions.

## [1.2.1] - 2026-04-16
### Changed
- Track name retrieval now prioritizes `GameData.TrackNameWithConfig` over `GameData.TrackName` if available.
- Readme update - Assetto Corsa Evo now works from version 0.6 and SimHub 9.11.11.

## [1.2.0] - 2026-04-12
### Added
- **LMU Tyre Properties Integration** — automatically detects and falls back to use the NeoRed plugin's properties for Le Mans Ultimate (LMU) if it is installed, fetching reliable data for all 4 tyre compounds without requiring manual override configuration.
- **AMS2 Tyre Compound Integration** — automatically reads tyre compound names from Automobilista 2's raw data properties (`mTyreCompound01`–`04`), providing accurate compound identification across all AMS2 tyre categories.
- Added 17 new default tyre compound categories spanning across Automobilista 2 and EA Sports WRC, natively mapping slick, wet, treaded, and dirt configurations with their own individual naming abbreviations and color mapping.
- Added a helpful direct hyperlink to the LMU NeoRed plugin threads alongside the Tyre Property Overrides section inside the settings window.
- **Auto-Refresh** — the lap records data table now automatically refreshes in real-time immediately when a new personal best lap time is posted, resolving the need to manually click the "Refresh Data" button.
- **Expand / Collapse All** — added toolbar buttons on the Lap Records tab to expand or collapse all track groups at once.

### Changed
- Refactored tyre compound retrieval into a game-agnostic fallback architecture. Game-specific fallback properties are resolved upfront and passed cleanly into `GetTyreVal`, making it trivial to add support for additional games.
- Default tyre compound definitions are now auto-merged on plugin boot. Any new compounds added to the default catalogue in future updates will automatically appear in the user's list without overwriting existing customizations.

## [1.1.0] - 2026-04-02
### Added
- **Track Name Overrides** — new "Track Overrides" tab with Add/Remove/inline-edit support. Map any raw sim track string (e.g. `monza_2024`) to a unified display name (e.g. `Monza`). Laps from multiple sims that share a unified name are stored in the same bucket, enabling true cross-sim records.
- `OriginalCarName` and `OriginalTrackName` fields added to `LapRecord`. Set once at record creation to capture the raw sim values before any override is applied. Existing records are retroactively stamped when an override is first added.
- `TrackNameOverrides` dictionary added to `Settings` for track merge persistence.

### Changed
- **Car Override unmerging** now uses `OriginalCarName` to identify which records belong to a specific override entry. Removing override "Car A" when "Car A" and "Car B" are both stored under the same unified name no longer incorrectly moves "Car B" laps.
- **Backup/Restore** now saves and restores the entire `Settings` object (lap records, car overrides, track overrides, tyre compound definitions, tyre property mappings, column visibility) in a single file. Legacy backups containing only lap records are still detected and restored correctly.
- Backup filename changed from `SimHub_LapRecords_*` to `SimHub_LapRecordPlugin_Backup_*` to reflect the broader scope.
- "Backup Lap Records" and "Restore Lap Records" buttons renamed to "Backup All Settings" and "Restore All Settings".

## [1.0.1] - 2026-03-29
### Added
- Expanded SimHub property endpoints extensively. The plugin now natively exposes `TrackState`, `TrackTemp`, `TyreFL`, `TyreFR`, `TyreRL`, `TyreRR`, and `Fuel` properties dynamically bound into the `CurrentCarBestLap` dataset.
- Added a brand new, fully populated `CurrentClassBestLap` property tree replicating the entire parameter set. It dynamically scans and tracks the absolute best lap time across all cars specifically matching your current car's Class designation locally.

### Changed
- Scaled down the plugin's stopwatch icon by enclosing the geometry inside a transparent bounding box, ensuring visual parity with native SimHub side-menu icons.
- Modified data injection and mapping loops across `Class`, `Session`, `Fuel Level`, and `Track Temp` columns to explicitly output correctly empty strings instead of default "Unknown" or zero placeholders when a game sim fails to provide targeted telemetry correctly.
- *Note:* `Track State` retains its `"Unknown"` default string behavior to prevent WPF DataGridComboBoxColumn items mapping corruption across completely unpopulated historical cells.
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
