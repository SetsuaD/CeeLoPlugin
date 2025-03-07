# CeeLoPlugin v1.1

**CeeLoPlugin** is a FFXIV plugin that allows a game dealer to track player participation and dice rolls for the Ceelo game. It supports player addition, roll recording, dynamic announcements in chat, tie-breaker mechanisms, and more.

## How to Use

1. **Configuration Tab:**
   - **Communication Mode Settings:** Choose the chat channel used for announcements.
   - **Game Parameters:** Set the GIL Bet and House Cut percentage.
   - **Log Folder Override:** Specify a custom folder for log files.

2. **Game (Roll Recording) Tab:**
   - **Advertise:** Announce introductory game instructions in chat.
   - **Player Management:** Add a target (if selected in-game), sort players, or add fake players.
   - **Player Table:** 
     - Enter roll orders, game rolls, and final scores.
     - Confirm bet receipt with the checkbox.
     - Use the crown button to designate a winner.
   - **Dealer Announcements:**
     - Announce new round, set round bet, announce pot total, announce last chance (with list of players who haven’t confirmed their bet), and close bets.
     - Roll order and turn announcements include options such as recall for roll order, handling ties (RollOrderTie), and calling next or re-rolling players.
   - **Tie Breaker Options:**
     - **Multi-Tie:** For three or more players tying, this button announces that the tied players must re-roll for order and re-score.
     - **Roulette:** For a two-player tie, this button announces a face-off “roulette” where the players roll dice to determine the winner.
   - **Winner Announcements:** Announce the current or final winner (final winner announcement includes the pot total).
   - **Game Controls:** Start a new game or clear the player table to restart the round.

3. **Log Tab:**
   - Save the log to a file.
   - Optionally add fake lines for testing.
   - Override the log folder path.

## Current Task List

- **[x] Tie Breaker Buttons (Multi-Tie & 2-Player Tie/Roulette):**  
  Implemented – see Tie Breaker Options row.

- **[x] Final Score Input Logic:**  
  Final score fields are now blank by default. A blank field is interpreted as “no score” so that a zero must be manually entered to count.

- **[x] Bet Confirm Checkbox:**  
  Added to the player table and integrated into the “Announce Last Chance” message.

- **[ ] Modularize Code:**  
  Consider refactoring code from `MainWindowV2.cs` into smaller, focused classes/files for better maintainability.

- **[ ] Auto-Receive Trades & Target Confirmation:**  
  Implement auto-receive trades and add a “confirm gil trade” button and amount field for automatically processing bet trades (code to be harvested from the Dropbox plugin).

- **[ ] Dynamic Plugin Update System:**  
  (Optional) Integrate a JSON-based dynamic update system if desired in future updates.

## Version History

- **v1.1:**  
  - Updated version record.
  - Added tie-breaker buttons and messages.
  - Improved final score and bet confirmation logic.
  - Updated announcements with proper formatting and pot total calculations.

---

*To update your plugin repository, commit these changes, tag the release as v1.1, and push to GitHub.*
