# CeeLoPlugin

CeeLoPlugin is a Final Fantasy XIV plugin that manages the Ceelo dice game. It helps track player participation, record dice rolls, announce status updates (like new rounds and bets), and finally determine the winner.

## Features

- **Dynamic Plugin Update**  
  Automatically checks for plugin updates using a JSON file.  
  Check for updates at: [Repo JSON](https://raw.githubusercontent.com/SetsuaD/CeeLoPlugin/master/Repo.json)

- **Player Management**  
  - Add players by targeting, or add fake players for testing.
  - Remove or sort players.
  - Confirm bet trades with a dedicated checkbox.

- **Roll Recording**  
  - Record roll order, three dice roll fields, and final scores.
  - Input final scores via editable fields (blank means no score yet).

- **Announcements & Tie-breakers**  
  - Announce new rounds, set bets, display pot totals, and call next players.
  - Handle tie-breaker scenarios:
    - **Multi-Tie**: For 3 or more players tying, prompt a re-roll order and re-scoring.
    - **Two-Player Tie (Roulette)**: For two players tying, initiate a roulette-style face-off.

- **Logging**  
  - Save in-game logs to a file and view them in a dedicated log tab.

## How to Use

1. **Setup & Configuration**  
   - Open the configuration UI via the `/ceelo` chat command or through Dalamud’s config interface.
   - Set game parameters like **GIL Bet** and **House Cut (%)**.
   - Optionally, set a custom log folder path.

2. **Gameplay Workflow**  
   - **Player Management:**  
     Use the "Add Target" button to add players, sort the list, or add fake players for testing.  
     Confirm bets by checking the Bet Confirm checkbox for each player.

   - **Roll Recording:**  
     Enter roll orders, record three dice rolls per player, and input the final score.  
     A blank final score indicates that the player hasn’t yet scored (even a score of 0 is considered a valid final score).

   - **Announcements:**  
     Use dedicated buttons to announce:
     - New rounds and bet details.
     - Current pot total (calculated as _Bet × Number of Players - House Cut%_).
     - Last chance messages (including names of players who haven’t confirmed their bet trades).
     - Roll order announcements, calling the next player, and re-rolling instructions.
     - Tie-breaker messages for both multi-tie and two-player tie (roulette) scenarios.

   - **Final Winner:**  
     Announce the final winner along with their final score and the pot total, then execute the trade.

## How to Play Ceelo

Ceelo is a dice game where players compete to achieve the highest score or specific combinations. Here’s a brief rundown:

1. **New Round:**  
   The dealer announces a new round with a bet range (e.g., "100k to 100m") and prompts players to suggest bets.

2. **Bet Setting:**  
   The dealer sets the round bet based on suggestions and announces the final bet.

3. **Pot Total Calculation:**  
   The dealer announces the pot total, which is calculated as:  
   **Pot Total = (Bet × Number of Players) - (House Cut%)**

4. **Finalizing Bets:**  
   A last chance announcement is made before closing bets. The announcement also lists players who have not yet confirmed their bet trade.

5. **Roll Order:**  
   Players roll for order (using `/random 99`) and then roll dice (using `/random 6` three times) in turn.  
   If a tie occurs:
   - **Multi-Tie:** Players tied will re-roll order and re-score.
   - **Two-Player Tie (Roulette):** The two tied players face off in a roulette-style challenge, where the first to roll a losing number loses.

6. **Final Scoring:**  
   The dealer enters final scores. A blank field means no score has been recorded, while a 0 is a valid score.  
   The next player is called only when the previous player’s score is settled.

7. **Winner Announcement:**  
   The final winner is announced along with their score and the total pot, and then the trade is executed.

## Version History

- **v1.1**
  - Dynamic plugin update enabled (JSON file available at [Repo JSON](https://raw.githubusercontent.com/SetsuaD/CeeLoPlugin/master/Repo.json)).
  - Tie-breaker buttons for multi-tie and two-player tie (roulette) scenarios added.
  - Improved bet confirmation handling and announcement messages.
  - Various bug fixes and UI enhancements.

## Installation

1. Clone or download the repository.
2. Ensure that ECommons is set up as a dependency.
3. Install the plugin using Dalamud’s plugin installer.
4. Enjoy playing Ceelo in FFXIV!

## Contributing

Contributions are welcome! Please fork the repository, submit issues, and open pull requests with your changes.

## License

This project is licensed under the [MIT License](LICENSE).

