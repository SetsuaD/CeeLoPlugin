# CeeLoPlugin

## Overview
CeeLoPlugin is a plugin for FFXIV that helps manage Ceelo game sessions by tracking player participation, dice rolls, bets, and more. It features dynamic plugin updates, improved UI, and several automated announcements for game events.

## Features
- **Dynamic Plugin Update:** Automatically checks for plugin updates via a JSON file.
- **Bet Management:** Players can confirm their bets using a checkbox; announcements include details of pending bet confirmations.
- **Dice Roll and Score Management:** Tracks game rolls, calculates final scores, and calls the next player automatically.
- **Tie-Breaker Handling:** Special buttons for handling multi-tie and two-player tie scenarios.
- **Detailed Announcements:** Announces new rounds, bet totals, roll orders, and final winners with clear, formatted text.
- **Logging:** Save logs and manage log folder paths.

## Commit Changelog (v1.1)
- **Dynamic Plugin Update:**  
  - Enabled dynamic update functionality using a JSON file (Repo.json) hosted at [Repo.json](https://raw.githubusercontent.com/SetsuaD/CeeLoPlugin/master/Repo.json).
- **Tie-Breaker Enhancements:**  
  - Added **Multi-Tie** button to handle scenarios with 3+ tied players.  
  - Added **Two-Player Tie (Roulette)** button to initiate a face-off for tied players with special roll rules.
- **Bet Confirmation:**  
  - Implemented a bet confirmation checkbox for each player in the table.  
  - Updated "Announce Last Chance" to include a list of players pending bet confirmation.
- **Roll Order and Next Player Logic:**  
  - Enhanced the "Call Next Player" functionality to announce the previous player's score along with calling the next player.
- **Final Winner Announcement:**  
  - Revised the announcement to include the current pot total and transfer instructions.
- **UI Enhancements & Bug Fixes:**  
  - Reorganized the UI button layout and table columns for clearer presentation.  
  - Improved number formatting (added commas for readability) and fixed final score handling.

## Task List
- [x] Implement dynamic plugin update via Repo.json.
- [x] Add tie-breaker buttons (Multi-Tie and Two-Player Tie) with appropriate announcements.
- [x] Add bet confirmation checkbox and update "Announce Last Chance" to list pending confirmations.
- [x] Enhance "Call Next Player" functionality to include the previous player's score.
- [x] Update final winner announcement to include the current pot total.
- [ ] (Ongoing) Consider auto-receive trades and additional trade confirmation UI.
- [ ] (Ongoing) Refactor UI logic into separate files for better maintainability.

## How to Use
1. **Configuration:**  
   - Set the chat channel, GIL Bet, and House Cut percentages.  
   - Optionally override the log folder path.
2. **Game Setup:**  
   - Add players via target selection or by adding fake players.  
   - Confirm bet trades using the bet confirmation checkbox.
3. **Game Flow:**  
   - Use the provided buttons to announce new rounds, set bets, call for rolls, and handle tie-breakers.
4. **Logging:**  
   - Save and review logs directly within the plugin.
5. **Updates:**  
   - The plugin automatically checks for updates via the provided Repo.json URL.

## Installation
Clone this repository and ensure that ECommons is set up as a submodule. Follow the build instructions provided in the repository.

## License
This project is licensed under the MIT License.
