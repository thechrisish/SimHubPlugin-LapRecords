The **Lap Record Plugin** for SimHub automatically tracks and logs your personal best lap times for each car and track combination.

## Disclaimer
Developed using Google Antigravity with Claude Sonnet 4.6 and Gemini Pro 3.1. The code has been human reviewed and tested. All files are available on this GitHub repository for you to confirm this. Use at your own risk.
The reason for using Claude and Gemini is because I need to learn it - my job is one that will fundamentally change with the onslaught of GenAI and simply, I need to keep up so I can pay my bills, so I figured why not do something I'll find useful in one of my passions - sim racing!

I can read and understand C# code, but I'm no good at writing it. Terraform, Ansible and Bash scripts are more my thing... With that in mind I'm more than happy for anyone to tear this apart and suggest where it can be improved.

This Readme IS human written, even if AI gave me a readable template to use. :smile:

<img width="1676" height="1379" alt="image" src="https://github.com/user-attachments/assets/06ec4d6d-7ffa-42f0-a6a1-ecbb581ac610" />

## Features
*   **Automatic Lap Logging:** Automatically records your Best lap times for every car and track combination in supported sims.
*   **Detailed Telemetry:** Logs useful data alongside your lap time:
    *   Game
    *   Session type
    *   Car class
    *   Car model
    *   Fuel level remaining
    *   Track temperature
    *   Track state (Dry, Intermediate, Wet)
    *   Tyre compounds used on all four wheels (See Limitations section below)
*   **Custom Tyre Support:** Define custom tyre compounds with their own abbreviations and colours for easy viewing. Lots of tyres included by default!
*   **Game-Specific Overrides:** Some games report tyre data differently, the plugin allows you to set up per-game overrides or natively falls back to specific properties (e.g. NeoRed for LMU).
*   **Car Name Overrides:** Fix car naming where Game Data reports it incorrectly.
*   **Track Name Overrides:** Map raw track strings (e.g. `monza_2024`) to something more generic (e.g. `Monza`) to merge records made in differnet sims under the same track name.
*   **Backup & Restore:** Easily backup all plugin settings, overrides, custom tyres, and lap records to a single file to keep them safe, or share them between installations.

## Limitations
* This plugin was developed mostly using Le Mans Ultimate but has been checked with most of the sims I own (see list below).
* Other sims may not have all the features available due to the way they report telemetry data. 
* Automatic tyre compounds are sometimes dependent on other plugins like NeoRed for LMU.
* Car names are reported to SimHub in different ways, you may want to use **Car Name Overrides** to display names correctly.
** Example 'AM Valkryie Custom Team 2025' you might want to correct to 'Aston Martin Valkryie AMR-LMH'

## Known Issues
* Tyre compound assignment overrides are a little fiddly.
* No idea what impact on SimHub performance is with lots of lap times. Please report issues if you suspect this plugin is causing lag in SimHub.

## Race Sims Tested (Accurate as of 15th April 2026)
* :white_check_mark: Automobilista 2 - Track State data not available.
* :white_check_mark: EA Sports WRC - Class, Session, Fuel, Track Temp, Track State and Tyre data not available.
* :white_check_mark: Le Mans Ultimate - NeoRed plugin used to automatically populate tyre compounds.
* :white_check_mark: rFactor 2
* :white_check_mark: Assetto Corsa Evo - ACE 0.6 and SimHub 9.11.11 Required.
* :x: Assetto Corsa Rally - No timing data in SimHub

If a sim is not listed here it doesn't mean they won't work, they're just untested. If you happen to test any for me then please let me know and I'll update the list.

## Installation
1.  Make sure SimHub is not running.
2.  Download the latest `SimHubLapRecordPlugin.dll` file.
3.  Copy the `.dll` file into your SimHub installation directory. 
    *   *By default, this is usually located at: `C:\Program Files (x86)\SimHub`*
4.  Start SimHub.
5.  If prompted, click **Enable** to activate the Lap Record Plugin.

## Update
1.  In SimHub > Lap Records > Settings - Press the **Backup All Settings...** button and save the file somewhere safe.
2.  Make sure SimHub is not running.
3.  Download the latest `SimHubLapRecordPlugin.dll` file.
4.  Copy the `.dll` file into your SimHub installation directory. 
    *   *This is usually located in `C:\Program Files (x86)\SimHub`*
5.  Start SimHub.
6.  If prompted, click **Enable** to activate the Lap Record Plugin.
7.  (If no laptimes appear) In SimHub > Lap Records > Settings - Press the **Restore All Settings...** button and select the backup file you saved in step 1.

## Usage
Once installed and enabled, a new **Lap Records** menu item will appear in the left-hand navigation bar of SimHub (look for the white stopwatch icon). 

### Viewing Records
*   Your lap times will automatically update as you set better times. You do not need to refresh manually.
*   The track you are driving on will always be at the top of the list.
*   Use the **Expand All** and **Collapse All** buttons to quickly manage the view of your track groups.
*   Use the **Game** dropdown to filter records for specific sims, or check the **Auto-filter to current game** box to only show records for the game you are currently running.
*   You can customize which columns of data are shown using the checkboxes inside the settings tab.

### Editing Tyre Information
If the game failed to record the tyres you used on a lap you can manually edit them:
1.  Click the small pencil icon (**✎**) next to the tyres for your target lap.
2.  Type the abbreviation for the tyre compound you used (`S` for Soft, `M` for Medium etc).
3.  Click anywhere else to save the changes.

### Customizing Tyres & Overrides
At the top of the Lap Records page, you can access the configuration settings tabs:
*   **Car Overrides:** Map one car name to another to combine their lap records. (Useful where SimHub reports a team name rather than a car name)
*   **Track Overrides:** Map raw sim track strings to a unified track name so they share a leaderboard. (Useful where a track is reported with different names in different sims)
*   **Settings > Custom Tyre Compounds:** Add or remove tyres and set their display colours.
*   **Settings > Tyre Property Overrides:** If a specific game isn't automatically showing tyre data, you can specify the exact SimHub properties the plugin should look at for that game. Games configured so far:
    *   LMU via NeoRed plugin 
    *   AMS2 has data natively in SimHub

## Data Backup
It's highly recommended to periodically back up your lap times, especially during updates. 
*   Click **Backup All Settings...** at the bottom of the page to save all your data to a JSON file.
*   Use **Restore All Settings...** to load a previously saved backup file. *Warning: Restoring will overwrite any currently stored data!*
