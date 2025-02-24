using CeeLoPlugin.Models;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeeLoPlugin.Logic
{
    public class CeeLoGameLogic
    {
        public List<Player> Players { get; private set; } = [];
        public List<int> PlayerBets { get; private set; } = []; // Track each player's bet
        public int PotTotal { get; private set; } = 0; // Total pot
        public List<(string Name, int Roll)> RollOrder { get; private set; } = []; // Store roll order with player names
        public bool HellRulesEnabled { get; private set; } = false; // Flag for Hell Rules
        public bool ReverseRulesEnabled { get; private set; } = false; // Flag for Reverse Rules
        public bool SixtyNineRulesEnabled { get; private set; } = false; // Flag for 69 Rules

        public void AddPlayer(string playerName, int betAmount)
        {
            if(Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
            {
                Services.ChatGui.PrintError($"{playerName} is already in the game.");
                return;
            }

            Players.Add(new Player(playerName));
            PlayerBets.Add(betAmount);
            PotTotal += betAmount;

            Services.ChatGui.Print($"{playerName} has been added to the game with a bet of {betAmount} gil.");
        }

        public void ConfirmBets()
        {
            if(Players.Count == 0)
            {
                Services.ChatGui.PrintError("No players have placed bets.");
                return;
            }

            var betSummary = string.Join(", ", Players.Zip(PlayerBets, (p, b) => $"{p.Name}: {b} gil"));
            Services.ChatGui.Print($"/shout Bet Summary: {betSummary}. Pot Total: {PotTotal} gil.");
            Services.ChatGui.Print("/shout Bets closing in 10 seconds!");
        }

        public void StartGame()
        {
            if(Players.Count == 0)
            {
                Services.ChatGui.PrintError("No players are ready to start the game.");
                return;
            }

            if(PotTotal == 0)
            {
                Services.ChatGui.PrintError("No bets have been placed.");
                return;
            }

            Services.ChatGui.Print("/shout The game of CeeLo is starting!");
            Services.ChatGui.Print($"/shout Total pot: {PotTotal} gil.");
            DetermineRollOrder();
        }

        public void DetermineRollOrder()
        {
            if(Players.Count == 0)
            {
                Services.ChatGui.PrintError("No players to roll.");
                return;
            }

            var random = new Random();
            RollOrder.Clear();

            foreach(var player in Players)
            {
                var roll = random.Next(1, 100); // Roll between 1-99
                RollOrder.Add((player.Name, roll));
            }

            RollOrder = RollOrder.OrderByDescending(r => r.Roll).ToList();

            var rollOrderAnnouncement = "/shout Roll Order: " + string.Join(" ", RollOrder.Select(r => r.Name));
            Services.ChatGui.Print(rollOrderAnnouncement);
            Services.ChatGui.Print("/shout Roll order for play! /random 99");
        }

        public void HandleRoll(string playerName, List<int> rollResults, int rollAttempt)
        {
            var score = CalculateScore(rollResults);
            var rollsDisplay = string.Join(",", rollResults);

            if(score == -1)
            {
                var message = rollAttempt == 3
                    ? $"/shout {playerName} has no scored 3 times."
                    : $"/shout {playerName} rolled {rollsDisplay} with no score! Roll again!";
                Services.ChatGui.Print(message);
            }
            else
            {
                var highScoreMessage = IsNewHighScore(score)
                    ? $"{score} points! New high score!"
                    : $"{score} points!";
                Services.ChatGui.Print($"/shout {playerName} rolled {rollsDisplay}. {highScoreMessage}");
            }
        }

        private int CalculateScore(List<int> rollResults)
        {
            if(rollResults.Count != 3)
                return -1;

            rollResults.Sort();

            if(rollResults[0] == rollResults[1] && rollResults[1] == rollResults[2])
            {
                return rollResults[0] + 10; // Triple bonus
            }

            if(rollResults.SequenceEqual([1, 2, 3]))
            {
                return -1; // Hard 123: Instant loss
            }

            if(rollResults.SequenceEqual([4, 5, 6]))
            {
                return 456; // Hard 456: Instant win
            }

            if(rollResults[0] == rollResults[1] || rollResults[1] == rollResults[2])
            {
                return rollResults.First(x => rollResults.Count(y => y == x) == 1);
            }

            return -1; // No score
        }

        private bool IsNewHighScore(int score)
        {
            return Players.All(p => p.Score < score);
        }

        public void ResetGame()
        {
            Players.Clear();
            PlayerBets.Clear();
            PotTotal = 0;
            RollOrder.Clear();
            Services.ChatGui.Print("The game has been reset.");
        }
    }
}
