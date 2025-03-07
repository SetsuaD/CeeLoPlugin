CeeLoPlugin
CeeLoPlugin is a Dalamud plugin for Final Fantasy XIV that helps dealers manage rounds of Ceelo. It allows you to track player participation, record dice rolls, manage bets, and automatically announce round status and winners via in-game chat.

Features
Player Management:

Add a target directly from your current in-game selection.
Add fake players for testing.
Remove or sort players as needed.
Dice Roll Recording:

Record individual dice rolls and enter final scores.
Automatically determine the roll order based on input.
Dealer Announcements:

Announce new rounds, set bets, and report pot totals (with proper comma formatting).
Call players to roll, announce roll order, and finalize winners.
Customizable chat messages for clear in-game instructions.
Logging:

Log all actions and events.
Save logs to a configurable folder for later review.
Dynamic Update Support:

The plugin supports dynamic updates via a JSON file hosted on GitHub.
Requirements
FFXIV with the Dalamud plugin framework.
ECommons library as a dependency.
Dalamud API level 11 (or as specified).
Installation
Download the latest release from the Releases page.
Extract the contents into your Dalamud plugins folder.
Restart FFXIV or reload Dalamud to load the plugin.
Usage
Open the Plugin UI:
Use the /ceelo command in-game to open the plugin's main window.

Tabs Overview:

Configuration Tab:
Set communication parameters, adjust game settings such as the bet amount (GIL Bet) and house cut percentage, and configure the log folder.

Game (Roll Recording) Tab:

Player Management Section:
Advertise: Announce a welcome message to new players before starting a game.
Add Target / Sort / Add Fake: Manage your player list.
Player Table: Record roll order, individual dice rolls, and final scores.
Dealer Announcements: Use dedicated buttons to:
Announce a new round.
Set the round bet.
Announce the current pot total (calculated as Bet × Player Count – House Cut).
Announce last chance to bet.
Announce that bets are closed.
Announce the roll order after sorting.
Call the next player to roll.
Request a reroll if needed.
Finalize and announce the winner.
Announce the current and final winner.
Log Tab:
View real-time log entries. Save or clear logs as needed.

In-Game Chat Commands:
All announcements are sent through in-game chat automatically based on button clicks in the UI. For example, clicking Announce New Round sends:

vbnet
Copy
New Round: 100k to 100m. What shall this round's bet be?
Other buttons work similarly, integrating variables like the bet amount, total pot (with commas for readability), and player names.

Development
Cloning the Repository with Submodules
To clone the repository along with its submodule (ECommons), run:

bash
Copy
git clone --recurse-submodules https://github.com/SetsuaD/CeeLoPlugin.git
If you have already cloned the repo without submodules, initialize them with:

bash
Copy
git submodule update --init --recursive
Building
Open the solution in Visual Studio.
Ensure that your references to ECommons are correctly resolved.
Build the solution in Debug/Release mode as needed.
Pushing Changes
When you’ve made changes:

Commit your changes locally.
Push to your GitHub repository.
Dynamic Update
A dynamic update JSON file is used to inform users about available updates. For example, the JSON file might look like this:

json
Copy
[
  {
    "Author": "SetsuaD",
    "Name": "CeeLoPlugin",
    "Punchline": "Track player participation and dice rolls in Ceelo with ease!",
    "Description": "CeeLoPlugin assists dealers in managing rounds of Ceelo by recording dice rolls, tracking bets, and automatically determining winners. It streamlines game management with integrated chat announcements and real-time data updates.",
    "Changelog": "Initial release with player management, dice roll recording, and chat announcement functionality.",
    "IsHide": false,
    "InternalName": "CeeLoPlugin",
    "AssemblyVersion": "1.0.0.0",
    "TestingAssemblyVersion": "1.0.0.0",
    "IsTestingExclusive": false,
    "RepoUrl": "https://github.com/SetsuaD/CeeLoPlugin",
    "ApplicableVersion": "any",
    "DalamudApiLevel": 11,
    "TestingDalamudApiLevel": 11,
    "DownloadCount": 0,
    "LastUpdate": 1740140791,
    "DownloadLinkInstall": "https://github.com/SetsuaD/CeeLoPlugin/releases/download/1.0/CeeloPluginV1.zip",
    "DownloadLinkUpdate": "https://github.com/SetsuaD/CeeLoPlugin/releases/download/1.0/CeeloPluginV1.zip",
    "DownloadLinkTesting": "https://github.com/SetsuaD/CeeLoPlugin/releases/download/1.0/CeeloPluginV1.zip",
    "SupportsProfiles": true,
    "IconUrl": "https://raw.githubusercontent.com/SetsuaD/CeeLoPlugin/main/Resources/icon.png"
  }
]
Make sure to update this file with each new release.

License
MIT License
