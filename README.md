# SimHub Lap Record Plugin

The **Lap Record Plugin** for SimHub automatically tracks and logs your personal best lap times across all your favourite racing simulators. It records detailed information about each of your best laps, allowing you to easily look back at your performance under varying conditions.

## Features

*   **Automatic Lap Logging:** Automatically records your Personal Best (PB) lap times for every car and track combination.
*   **Detailed Telemetry:** Logs essential session data alongside your lap time, including:
    *   Game & Session type
    *   Car class and specific car model
    *   Fuel level remaining
    *   Track temperature and track state (Dry, Intermediate, Wet)
    *   Tyre compounds used on all four wheels
*   **Custom Tyre Support:** Define custom tyre compounds with their own abbreviations and colours for easy viewing.
*   **Game-Specific Overrides:** Some games report tyre data uniquely. The plugin allows you to set up per-game overrides to ensure your tyre information is captured correctly.
*   **Car Name Aliasing:** Combine records from the same car if it's reported differently by the game (e.g., merging "Porsche 911 GT3 R" and "Porsche GT3").
*   **Backup & Restore:** Easily backup your lap records to a file to keep them safe, or share them between installations.

## Installation

1.  Make sure SimHub is completely closed.
2.  Download the latest `SimHubLapRecordPlugin.dll` file.
3.  Copy the `.dll` file into your SimHub installation directory. 
    *   *By default, this is usually located at: `C:\Program Files (x86)\SimHub`*
4.  Start SimHub.
5.  If prompted, click **Enable** to activate the Lap Record Plugin.

## Usage

Once installed and enabled, a new **Lap Records** menu item will appear in the left-hand navigation bar of SimHub (look for the white stopwatch icon). 

### Viewing Records
*   Your lap times will automatically populate as you set new Personal Bests in your games.
*   Use the **Game** dropdown to filter records for specific simulators, or check the **Auto-filter to current game** box to only show records for the game you are currently running.
*   You can customize which columns of data are shown using the checkboxes on the right side of the screen.

### Editing Tyre Information
If the game failed to accurately record the tyres you used on a lap, you can edit them manually:
1.  Click the small pencil icon (**✎**) next to the tyres for a specific lap record.
2.  Type the abbreviation for the tyre compound you used (e.g., `S` for Soft, `M` for Medium).
3.  Click anywhere else to save the changes.

### Customizing Tyres & Overrides
At the bottom of the Lap Records page, you can access the configuration settings:
*   **Custom Tyre Compounds:** Add or remove tyres and set their display colours.
*   **Car Name Overrides:** Map one car name to another to combine their lap records.
*   **Tyre Property Overrides:** If a specific game isn't automatically showing tyre data, you can specify the exact SimHub properties the plugin should look at for that game.

## Data Backup
It's highly recommended to periodically back up your lap times. 
*   Click **Backup Records** at the bottom of the page to save all your data to a JSON file.
*   Use **Restore Records** to load a previously saved backup file. *(Warning: Restoring will overwrite any currently stored laps!)*
