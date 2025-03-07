# CeeLoPlugin

CeeLoPlugin is a Dalamud plugin for Final Fantasy XIV that helps dealers manage rounds of Ceelo. It allows you to track player participation, record dice rolls, manage bets, and automatically announce round status and winners via in-game chat.

## Features

- **Player Management:**  
  - Add a target directly from your current in-game selection.
  - Add fake players for testing.
  - Remove or sort players as needed.

- **Dice Roll Recording:**  
  - Record individual dice rolls and enter final scores.
  - Automatically determine roll order based on input.

- **Dealer Announcements:**  
  - Announce new rounds, set bets, and report pot totals (with proper comma formatting).
  - Call players to roll, announce roll order, and finalize winners.
  - Customizable chat messages for clear in-game instructions.

- **Logging:**  
  - View real-time log entries for game events.

## How to Use

1. **Opening the Plugin UI:**  
   - Use the `/ceelo` command in-game to open the plugin's main window.
   - The plugin has three main tabs:
     - **Configuration Tab:**  
       Configure communication settings, adjust game parameters (bet amount and house cut), and set the log folder path.
     - **Game (Roll Recording) Tab:**  
       Manage players, record dice rolls, and use dealer announcement buttons.
     - **Log Tab:**  
       View and save logs of game events.

2. **Player Management:**  
   - **Advertise:**  
     Send a welcome message and instructions before starting a game.
   - **Add Target:**  
     Add the currently selected player to the game.
   - **Sort:**  
     Sort players based on roll order.
   - **Add Fake:**  
     Add fake players for testing purposes.
   - **Remove Player:**  
     Use the trash icon next to a player's name to manually remove them from the list.

3. **Dice Roll Recording and Announcements:**  
   - Enter individual roll values and the final score for each player in the provided fields.
   - Use buttons to:
     - **Announce New Round:**  
       Prompt players to suggest bets.
     - **Set Round Bet:**  
       Announce the current round bet.
     - **Announce Pot Total:**  
       Display the current pot total, calculated as `Bet × Player Count – House Cut` (numbers are formatted with commas).
     - **Announce Last Chance:**  
       Alert players that it is their final chance to place bets.
     - **Announce Bets Closed:**  
       Notify that no further bets will be accepted.
     - **Announce Roll Order:**  
       Automatically sort and announce the player roll order.
     - **Call Next Player / Roll Again:**  
       Prompt players to roll or reroll as necessary.
     - **Finalize Winner:**  
       Announce the winner and the final pot (with additional manual verification if required).
     - **Announce Current Winner:**  
       Show the current highest scoring player.
     - **Announce Final Winner:**  
       Confirm the final winner at the end of the round.

4. **In-Game Chat:**  
   All announcements are sent via in-game chat, with numbers formatted with commas for readability.

## How to Play Ceelo

Ceelo is a dice game where players compete against each other in rounds. The basic rules are as follows:

1. **Starting a Round:**  
   The dealer announces a new round with a betting range (e.g., 100k to 100m gil) and asks for players' bet suggestions.

2. **Placing Bets:**  
   After suggestions, the dealer sets the round bet and instructs players to place their bets accordingly.

3. **Rolling for Order:**  
   Players roll dice to determine the roll order.  
   - Use `/random 99` to decide order.
   - In the event of a tie, a tie breaker is conducted until a unique order is established.

4. **Rolling for Score:**  
   Each player takes turns to roll the dice (typically using `/random 6 three times` for their turn).
   - The final score is recorded for each player.
   - If a player scores 0, they may be prompted to roll again.

5. **Determining the Winner:**  
   Once all players have rolled:
   - The player with the highest final score is declared the winner.
   - The pot is calculated as `(Bet × Number of Players) – House Cut`.
   - The winner receives the pot (further manual confirmation of bet payment may be required).

6. **Additional Announcements:**  
   The plugin supports further dynamic announcements, such as reminding players to complete their roll order or instructing players to trade the dealer directly for bets when the pot is locked in.

Enjoy playing Ceelo and managing your game with ease using CeeLoPlugin!

---

Happy Gaming!
