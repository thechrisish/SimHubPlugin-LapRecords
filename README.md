<img width="1676" height="1379" alt="image" src="https://github.com/user-attachments/assets/6d7e3e7d-c3eb-4b90-9e3e-5c36ac3cd3dc" /># SimHub Lap Record Plugin

The **Lap Record Plugin** for SimHub automatically tracks and logs your personal best lap times for each car and track combination.

## Disclaimer
Developed using Google Antigravity with Claude Sonnet 4.5 and Gemini Pro 3.1. The code has been human reviewed and tested. All files are available on this GitHub repository for you to confirm this. Use at your own risk.
The reason for using Claude and Gemini is because I need to learn it - my job is one that will fundamentally change with the onslaught of GenAI and simply, I need to keep up so I can pay my bills, so I figured why not do something I'll find useful in one of my passions - sim racing!

I can read and understand C# code, but I'm no good at writing it. Terraform, Ansible and Bash scripts are more my thing... With that in mind I'm more than happy for anyone to tear this apart and suggest where it can be improved.

This Readme IS human written, even if AI gave me a readable template to use. :smile:

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
*   **Custom Tyre Support:** Define custom tyre compounds with their own abbreviations and colours for easy viewing.
*   **Game-Specific Overrides:** Some games report tyre data differently, the plugin allows you to set up per-game overrides to ensure your tyre information is captured correctly.
*   **Car Name Overrides:** Fix car naming where Game Data reports it incorrectly.
*   **Track Name Overrides** (From 1.1.0) Override track names so you don't have every sim's interpretation of how they name Monza GP.
*   **Backup & Restore:** Easily backup your lap records and settings to a file to keep them safe. During my development a change wiped all my laptimes - you have been warned!

## Limitations
* This plugin was developed mostly using Le Mans Ultimate but has been checked with most of the sims I own (see list below).
* Other sims may not have all the features available due to the way they report telemetry data. 
* Automatic tyre compounds is dependent on other plugins like NeoRed for LMU, SimHub does not natively have tyre compounds available that I could see!
* Car names are reported to SimHub in different ways, you may want to use **Car Name Overrides** to display names correctly.
** Example 'AM Valkryie Custom Team 2025' you might want to correct to 'Aston Martin Valkryie AMR-LMH'

## Known Issues
* Car Overrides list disappears on update, but somehow still works...?
* Tyre compound assignment overrides are a little fiddly.
* When a laptime is updated the screen doesn't automatically refresh.
* Different sims name different tracks differently... I may add a future improvement to allow merging of tracks in the list.
* When properties of a recorded time are unknown could be displayed better.

## Race Sims Tested (Accurate as of 29th March 2026)
* :white_check_mark: Automobilista 2 - Track State data not available.
* :white_check_mark: EA Sports WRC - Class, Session, Fuel, Track Temp, Track State and Tyre data not available.
* :white_check_mark: Le Mans Ultimate - NeoRed plugin used to automatically populate tyre compounds.
* :white_check_mark: rFactor 2
* :x: Assetto Corsa Evo - No timing data in SimHub
* :x: Assetto Corsa Rally - No timing data in SimHub

## Installation
1.  Download the `SimHubLapRecordPlugin.dll` from the [releases](https://github.com/thechrisish/SimHubPlugin-LapRecords/releases) page. 
2.  Make sure SimHub is not running.
3.  Download the latest `SimHubLapRecordPlugin.dll` file.
4.  Copy the `.dll` file into your SimHub installation directory. 
    *   *This is usually located in `C:\Program Files (x86)\SimHub`*
5.  Start SimHub.
6.  If prompted, click **Enable** to activate the Lap Record Plugin.

## Update
1.  Download the `SimHubLapRecordPlugin.dll` from the [releases](https://github.com/thechrisish/SimHubPlugin-LapRecords/releases) page. 
2.  In SimHub > Lap Records > Settings - Press the **Backup Lap Records...** button and save the file somewhere safe.
3.  Make sure SimHub is not running.
4.  Download the latest `SimHubLapRecordPlugin.dll` file.
5.  Copy the `.dll` file into your SimHub installation directory, overwriting the existing file.
    *   *This is usually located in `C:\Program Files (x86)\SimHub`*
6.  Start SimHub.
7.  If prompted, click **Enable** to activate the Lap Record Plugin.
8.  (If no laptimes appear) In SimHub > Lap Records > Settings - Press the **Restore Lap Records...** button and select the backup file you saved in step 1.

## Usage
Once installed and enabled, a new **Lap Records** menu item will appear in the left-hand navigation bar of SimHub (look for the white stopwatch icon). 

### Viewing Records
*   Your lap times will automatically update as you set better times.
*   The track you are driving on will always be at the top of the list.
*   Use the **Game** dropdown to filter records for specific sims, or check the **Auto-filter to current game** box to only show records for the game you are currently running.
*   You can customize which columns of data are shown using the checkboxes inside the settings tab.

### Editing Tyre Information
If the game failed to record the tyres you used on a lap you can manually edit them:
1.  Click the small pencil icon (**✎**) next to the tyres for your target lap.
2.  Type the abbreviation for the tyre compound you used (`S` for Soft, `M` for Medium etc).
3.  Click anywhere else to save the changes.

### Customising Tyres & Overrides
At the top of the Lap Records page, you can access the configuration settings:
*   **Car Name Overrides:** Map one car name to another to combine their lap records.
*   **Custom Tyre Compounds:** Add or remove tyres and set their display colours.
*   **Tyre Property Overrides:** If a specific game isn't automatically showing tyre data, you can specify the exact SimHub properties the plugin should look at for that game. (NeoRed plugin for LMU has this data)

## Data Backup
It's highly recommended to periodically back up your lap times, especially during updates. 
*   Click **Backup Records** at the bottom of the page to save all your data to a JSON file.
*   Use **Restore Records** to load a previously saved backup file. *Warning: Restoring will overwrite any currently stored laps!*

<img width="1676" height="1379" alt="image" src="https://github.com/user-attachments/assets/06ec4d6d-7ffa-42f0-a6a1-ecbb581ac610" />
