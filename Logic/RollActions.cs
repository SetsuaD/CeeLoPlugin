using System;
using System.Linq;
using CeeLoPlugin.UIv2.DataStructures;
using ECommons.ImGuiMethods;

namespace CeeLoPlugin.Logic
{
    public static class RollActions
    {
        // Announcement Methods
        public static void AnnounceNewRound()
        {
            Plugin.Instance.ChatSender.EnqueueMessage("New Round: 100k to 100m. At least 3 players must agree on the bet to set the round bet. What shall this round's bet be?");
        }

        public static void SetRoundBet()
        {
            Plugin.Instance.ChatSender.EnqueueMessage($"This round's bet is {Plugin.Instance.Configuration.GilBet:N0}. Place your bets!");
        }

        public static void AnnouncePotTotal()
        {
            int totalPot = GetPotTotal();
            Plugin.Instance.ChatSender.EnqueueMessage($"Current pot total is {totalPot:N0}.");
        }

        public static void AnnounceLastChance()
        {
            var notConfirmed = Plugin.Instance.Configuration.PlayerDatas
                .Where(p => !p.BetCollected)
                .Select(p => p.NameWithWorld);
            if (notConfirmed.Any())
            {
                string notConfirmedList = string.Join(", ", notConfirmed);
                Plugin.Instance.ChatSender.EnqueueMessage($"Last chance to place your bet. Closing Soon™! Waiting on: {notConfirmedList} for bet trade still.");
            }
            else
            {
                Plugin.Instance.ChatSender.EnqueueMessage("Last chance to place your bet. Closing Soon™!");
            }
        }

        public static void AnnounceBetsClosed()
        {
            int totalPot = GetPotTotal();
            Plugin.Instance.ChatSender.EnqueueMessage($"Bets CLOSED. Current Pot Total is: {totalPot:N0}. Moving to Roll Order Phase! (/random 99)");
        }

        // --- Roll Order & Turn Methods ---
        public static void RecallForRollOrder()
        {
            var missingRollOrderPlayers = Plugin.Instance.Configuration.PlayerDatas
                .Where(p => p.OrderDeterminingRolls.Count == 0 || p.OrderDeterminingRolls[0] == 0)
                .Select(p => p.NameWithWorld);
            if (missingRollOrderPlayers.Any())
            {
                string names = string.Join(", ", missingRollOrderPlayers);
                Plugin.Instance.ChatSender.EnqueueMessage($"Waiting on Roll Order Roll for: {names}. Please /random 99 for Roll Order.");
            }
        }

        public static void RollOrderTie()
        {
            var tiedGroups = Plugin.Instance.Configuration.PlayerDatas
                .Where(p => p.OrderDeterminingRolls.Count > 0)
                .GroupBy(p => p.OrderDeterminingRolls[0])
                .Where(g => g.Count() > 1 && g.Key != 0);
            if (tiedGroups.Any())
            {
                var tiedPlayers = tiedGroups.SelectMany(g => g)
                    .Select(p => p.NameWithWorld)
                    .Distinct();
                string tiedList = tiedPlayers.Any() ? string.Join(", ", tiedPlayers) : "None";
                Plugin.Instance.ChatSender.EnqueueMessage($"Players {tiedList} please reroll. (/random 99)");
            }
        }

        public static void AnnounceRollOrder()
        {
            Plugin.Instance.Configuration.PlayerDatas = Plugin.Instance.Configuration.PlayerDatas
                .OrderByDescending(x => x.OrderDeterminingRolls != null
                    ? string.Join("", x.OrderDeterminingRolls.Select(s => s.ToString("D3")))
                    : "")
                .ToList();
            var rollOrder = string.Join(", ", Plugin.Instance.Configuration.PlayerDatas.Select(x => x.NameWithWorld));
            Plugin.Instance.ChatSender.EnqueueMessage($"Roll order sorted! Current roll order is: {rollOrder}.");
        }

        public static void CallNextPlayer()
        {
            var nextPlayer = Plugin.Instance.Configuration.PlayerDatas.FirstOrDefault(x => x.FinalScore == null);
            if (nextPlayer != null)
            {
                var recentPlayer = Plugin.Instance.Configuration.PlayerDatas.LastOrDefault(x => x.FinalScore.HasValue);
                string recentText = recentPlayer != null
                    ? $"{recentPlayer.NameWithWorld} has scored a {recentPlayer.FinalScore.Value}. "
                    : "";
                Plugin.Instance.ChatSender.EnqueueMessage($"{recentText}{nextPlayer.NameWithWorld}, Roll your dice! (/random 6 three times)");
            }
            else
            {
                Notify.Error("All players have rolled.");
            }
        }

        // --- Updated RollAgain Logic ---
        public static void RollAgain()
        {
            var nextPlayer = Plugin.Instance.Configuration.PlayerDatas.FirstOrDefault(x => x.FinalScore == null);
            if (nextPlayer == null)
            {
                Notify.Error("All players have rolled.");
                return;
            }
            if (nextPlayer.GameRolls == null || nextPlayer.GameRolls.Length < 3)
            {
                Notify.Error("Rolls are not properly configured.");
                return;
            }
            int nextRollIndex = System.Array.FindIndex(nextPlayer.GameRolls, r => r == 0);
            if (nextRollIndex == -1)
            {
                Notify.Info($"{nextPlayer.NameWithWorld} has completed all rolls.");
                return;
            }
            if (nextRollIndex == 0 && nextPlayer.GameRolls.All(r => r == 0))
            {
                Notify.Info("No roll has been made yet.");
                return;
            }
            string rollName = nextRollIndex switch
            {
                0 => "first",
                1 => "second",
                2 => "third",
                _ => "next"
            };
            Plugin.Instance.ChatSender.EnqueueMessage($"{nextPlayer.NameWithWorld}, please roll for your {rollName} roll. (/random 6 three times)");
        }

        // --- Winner Announcement Methods ---
        public static void AnnounceCurrentWinner()
        {
            int maxScore = Plugin.Instance.Configuration.PlayerDatas.Max(p => p.FinalScore ?? 0);
            var tiedPlayers = Plugin.Instance.Configuration.PlayerDatas.Where(p => (p.FinalScore ?? 0) == maxScore).ToList();
            if (tiedPlayers.Count > 1)
            {
                bool allCrowned = tiedPlayers.All(p => p.IsWinner);
                string names = string.Join(", ", tiedPlayers.Select(p => p.NameWithWorld));
                if (allCrowned)
                    Plugin.Instance.ChatSender.EnqueueMessage($"Tie detected: {names} have the same top score of {maxScore:N0} and have all marked their crowns. They must resolve the tie.");
                else
                    Plugin.Instance.ChatSender.EnqueueMessage($"Tie detected: {names} have the same top score of {maxScore:N0}. Please resolve the tie.");
            }
            else if (tiedPlayers.Count == 1)
            {
                var winner = tiedPlayers.First();
                Plugin.Instance.ChatSender.EnqueueMessage($"{winner.NameWithWorld} is the highest score with {winner.FinalScore?.ToString("N0") ?? "No Score"}.");
            }
            else
            {
                Notify.Error("No current winner.");
            }
        }

        public static void AnnounceFinalWinner()
        {
            // If exactly two players are in the game, check for a tie in scores first.
            if (Plugin.Instance.Configuration.PlayerDatas.Count == 2)
            {
                var players = Plugin.Instance.Configuration.PlayerDatas;
                int score1 = players[0].FinalScore ?? 0;
                int score2 = players[1].FinalScore ?? 0;

                if (score1 == score2)
                {
                    // Scores are tied—use crown status to decide the tie-breaker.
                    bool p1Crown = players[0].IsWinner;
                    bool p2Crown = players[1].IsWinner;
                    if (p1Crown && !p2Crown)
                        Plugin.Instance.ChatSender.EnqueueMessage($"{players[0].NameWithWorld} wins the roulette tie-breaker!");
                    else if (!p1Crown && p2Crown)
                        Plugin.Instance.ChatSender.EnqueueMessage($"{players[1].NameWithWorld} wins the roulette tie-breaker!");
                    else
                        Notify.Error("Ambiguous crown selection. Please resolve the tie-breaker by ensuring only one crown is selected.");
                }
                else
                {
                    // No tie in scores: announce the player with the higher score.
                    var finalWinner = players.OrderByDescending(x => x.FinalScore ?? 0).First();
                    int totalPot = GetPotTotal();
                    Plugin.Instance.ChatSender.EnqueueMessage($"{finalWinner.NameWithWorld} is the final winner with {finalWinner.FinalScore?.ToString("N0") ?? "No Score"}. They win the pot totalling {totalPot:N0}! Trading {finalWinner.NameWithWorld} Now!");
                }
            }
            else
            {
                // For more than 2 players, simply pick the one with the highest final score.
                var finalWinner = Plugin.Instance.Configuration.PlayerDatas.OrderByDescending(x => x.FinalScore ?? 0).FirstOrDefault();
                if (finalWinner != null)
                {
                    int totalPot = GetPotTotal();
                    Plugin.Instance.ChatSender.EnqueueMessage($"{finalWinner.NameWithWorld} is the final winner with {(finalWinner.FinalScore.HasValue ? finalWinner.FinalScore.Value.ToString("N0") : "No Score")}. They win the pot totalling {totalPot:N0}! Trading {finalWinner.NameWithWorld} Now!");
                }
                else
                {
                    Notify.Error("No final winner.");
                }
            }
        }

        // --- Tie Resolution Methods ---
        // This method simply announces the tie situation.
        public static void ResolveTie()
        {
            var crownedPlayers = Plugin.Instance.Configuration.PlayerDatas.Where(p => p.IsWinner).ToList();
            // For a 2-player tie, use the recalculated pot total.
            int totalPot = GetPotTotal();
            if (crownedPlayers.Count == 2)
            {
                string names = string.Join(" and ", crownedPlayers.Select(p => p.NameWithWorld));
                Plugin.Instance.ChatSender.EnqueueMessage($"{names} have tied for a pot of {totalPot:N0} gil! Please choose: Split or Blood?");
                Notify.Info($"Dealer: The trade gil box is prepopulated with {totalPot:N0} gil.");
            }
            else if (crownedPlayers.Count >= 3)
            {
                // For multi-tie, keep the original gil bet value
                int originalBet = Plugin.Instance.Configuration.GilBet;
                string names = string.Join(", ", crownedPlayers.Select(p => p.NameWithWorld));
                Plugin.Instance.ChatSender.EnqueueMessage($"Multi-tie detected among: {names} for a pot of {originalBet:N0} gil. A fresh game will start with these players only.");
                Notify.Info($"Dealer: The trade gil box is prepopulated with {originalBet:N0} gil.");
            }
            else
            {
                Notify.Error("Tie resolution error: Exactly 2 or more players must be crowned to resolve a tie.");
            }
        }

        public static void ResolveTieSplit()
        {
            var crownedPlayers = Plugin.Instance.Configuration.PlayerDatas.Where(p => p.IsWinner).ToList();
            int totalPot = GetPotTotal();
            if (crownedPlayers.Count == 2)
            {
                string names = string.Join(" and ", crownedPlayers.Select(p => p.NameWithWorld));
                Plugin.Instance.ChatSender.EnqueueMessage($"{names} have chosen to split the pot of {totalPot:N0} gil equally!");
            }
            else
            {
                Notify.Error("Split tie resolution requires exactly 2 crowned players.");
            }
        }

        public static void ResolveTieRoulette()
        {
            var crownedPlayers = Plugin.Instance.Configuration.PlayerDatas.Where(p => p.IsWinner).ToList();
            int totalPot = GetPotTotal();
            if (crownedPlayers.Count == 2)
            {
                string names = string.Join(" and ", crownedPlayers.Select(p => p.NameWithWorld));
                Plugin.Instance.ChatSender.EnqueueMessage($"{names} have chosen sudden death roulette for the pot of {totalPot:N0} gil! May the odds be ever in your favor!");
                Plugin.Instance.ChatSender.EnqueueMessage(
                    "Tie Breaker Rules\n" +
                    "In the event of a tie between two players at the end of a round, the tie will be resolved with a Roulette Showdown.\n" +
                    "Roulette Showdown:\n" +
                    "- If the tied players agree, they can opt to split the pot evenly instead of proceeding with the showdown.\n" +
                    "Roulette Game Rules:\n" +
                    "- Players roll /random 99 to determine the first turn. The player with the highest roll goes first.\n" +
                    "- Turn 1 rolls /random 6.\n" +
                    "- Turn 2 rolls /random 5.\n" +
                    "- Turn 3 rolls /random 4.\n" +
                    "- Turn 4 rolls /random 3.\n" +
                    "- Turn 5 rolls /random 2.\n" +
                    "- Turn 6 loses by default.\n" +
                    "- If a player rolls a 1, they are eliminated."
                );
            }
            else
            {
                Notify.Error("Roulette tie resolution requires exactly 2 crowned players.");
            }
        }

        public static void ResolveMultiTie()
        {
            var crownedPlayers = Plugin.Instance.Configuration.PlayerDatas.Where(p => p.IsWinner).ToList();
            int originalBet = Plugin.Instance.Configuration.GilBet; // Keep the original gil bet value
            if (crownedPlayers.Count >= 3)
            {
                string names = string.Join(", ", crownedPlayers.Select(p => p.NameWithWorld));
                Plugin.Instance.ChatSender.EnqueueMessage($"Multi-tie detected among: {names} for a pot of {originalBet:N0} gil. A fresh game will now start with these players only.");
                // Remove any players not crowned.
                Plugin.Instance.Configuration.PlayerDatas = crownedPlayers;
                Notify.Info($"Dealer: The trade gil box remains set to {originalBet:N0} gil.");
            }
            else
            {
                Notify.Error("Multi-tie resolution requires at least 3 crowned players.");
            }
        }


        // --- Helper Method ---
        private static int GetPotTotal()
        {
            int baseTotal = Plugin.Instance.Configuration.GilBet * Plugin.Instance.Configuration.PlayerDatas.Count;
            int houseCutAmount = (int)(baseTotal * (Plugin.Instance.Configuration.HouseCut / 100f));
            return baseTotal - houseCutAmount;
        }
    }
}
