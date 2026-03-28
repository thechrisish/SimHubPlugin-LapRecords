# SimHub Lap Record Plugin

The **Lap Record Plugin** for SimHub automatically tracks and logs your personal best lap times for each track and car combination.

## Disclaimer
Developed using Google Antigravity with Claude Sonnet 4.5 and Gemini Pro 3.1. The code has been human reviewed and tested and is safe to use. All files are available on this GitHub repository for you to confirm this. Use at your own risk. 

## Features

*   **Automatic Lap Logging:** Automatically records your Personal Best (PB) lap times for every car and track combination.
*   **Detailed Telemetry:** Logs essential session data alongside your lap time, including:
    *   Game & Session type
    *   Car class and specific car model
    *   Fuel level remaining
    *   Track temperature and track state (Dry, Intermediate, Wet)
    *   Tyre compounds used on all four wheels (See Limitations section below)
*   **Custom Tyre Support:** Define custom tyre compounds with their own abbreviations and colours for easy viewing.
*   **Game-Specific Overrides:** Some games report tyre data uniquely. The plugin allows you to set up per-game overrides to ensure your tyre information is captured correctly.
*   **Car Name Overrides:** Fix car naming where Game Data reports it incorrectly.
*   **Backup & Restore:** Easily backup your lap records to a file to keep them safe, or share them between installations.

## Limitations
* This plugin was developed mostly using Le Mans Ultimate but has been checked with rFactor2 and Automobilista 2.
* Other sims may not have all the features available due to the way they report telemetry data. 
* Automatic tyre compounds is dependent on other plugins like NeoRed for LMU, SimHub does not natively have tyre compounds available that I could see!
* Car names are reported to SimHub in different ways, you may want to use **Car Name Overrides** to display names correctly.
** Example 'AM Valkryie Custom Team 2025' you might want to correct to 'Aston Martin Valkryie AMR-LMH'

## Known Issues
* Car Overrides list disappears on update, but somehow still works?

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
At the top of the Lap Records page, you can access the configuration settings:
*   **Car Name Overrides:** Map one car name to another to combine their lap records.
*   **Custom Tyre Compounds:** Add or remove tyres and set their display colours.
*   **Tyre Property Overrides:** If a specific game isn't automatically showing tyre data, you can specify the exact SimHub properties the plugin should look at for that game. (NeoRed plugin for LMU has this data)

## Data Backup
It's highly recommended to periodically back up your lap times. 
*   Click **Backup Records** at the bottom of the page to save all your data to a JSON file.
*   Use **Restore Records** to load a previously saved backup file. *(Warning: Restoring will overwrite any currently stored laps!)*
