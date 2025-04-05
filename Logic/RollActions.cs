using System.Linq;
using CeeLoPlugin.UIv2.DataStructures;
using ECommons.ImGuiMethods;

namespace CeeLoPlugin.Logic
{
    public static class RollActions
    {
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
            int baseTotal = Plugin.Instance.Configuration.GilBet * Plugin.Instance.Configuration.PlayerDatas.Count;
            int houseCutAmount = (int)(baseTotal * (Plugin.Instance.Configuration.HouseCut / 100f));
            int totalPot = baseTotal - houseCutAmount;
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
            int baseTotal = Plugin.Instance.Configuration.GilBet * Plugin.Instance.Configuration.PlayerDatas.Count;
            int houseCutAmount = (int)(baseTotal * (Plugin.Instance.Configuration.HouseCut / 100f));
            int totalPot = baseTotal - houseCutAmount;
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

        public static void RollAgain()
        {
            var nextPlayer = Plugin.Instance.Configuration.PlayerDatas.FirstOrDefault(x => x.FinalScore == null);
            if (nextPlayer != null)
            {
                if (nextPlayer.GameRolls.Length >= 3 &&
                    nextPlayer.GameRolls[0] != 0 &&
                    nextPlayer.GameRolls[1] == 0 &&
                    nextPlayer.GameRolls[2] == 0)
                {
                    Plugin.Instance.ChatSender.EnqueueMessage($"{nextPlayer.NameWithWorld}, No score. Roll again. (/random 6 three times)");
                }
                else
                {
                    Notify.Info($"{nextPlayer.NameWithWorld} is not eligible for a reroll at this stage.");
                }
            }
            else
            {
                Notify.Error("All players have rolled.");
            }
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
                    Plugin.Instance.ChatSender.EnqueueMessage($"Tie detected: {names} have the same top score of {maxScore:N0} and have all marked their crowns. They will split the winnings.");
                else
                    Plugin.Instance.ChatSender.EnqueueMessage($"Tie detected: {names} have the same top score of {maxScore:N0}. Please decide to either split the winnings or choose a roulette tie-breaker.");
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
            if (Plugin.Instance.Configuration.PlayerDatas.Count == 2)
            {
                var players = Plugin.Instance.Configuration.PlayerDatas;
                bool p1Crown = players[0].IsWinner;
                bool p2Crown = players[1].IsWinner;
                if (p1Crown && !p2Crown)
                    Plugin.Instance.ChatSender.EnqueueMessage($"{players[0].NameWithWorld} wins the roulette tie-breaker!");
                else if (!p1Crown && p2Crown)
                    Plugin.Instance.ChatSender.EnqueueMessage($"{players[1].NameWithWorld} wins the roulette tie-breaker!");
                else
                    Notify.Error("Ambiguous crown selection. Please resolve tie-breaker by ensuring only one crown is selected.");
            }
            else
            {
                var finalWinner = Plugin.Instance.Configuration.PlayerDatas.OrderByDescending(x => x.FinalScore ?? 0).FirstOrDefault();
                if (finalWinner != null)
                {
                    int baseTotal = Plugin.Instance.Configuration.GilBet * Plugin.Instance.Configuration.PlayerDatas.Count;
                    int houseCutAmount = (int)(baseTotal * (Plugin.Instance.Configuration.HouseCut / 100f));
                    int totalPot = baseTotal - houseCutAmount;
                    Plugin.Instance.ChatSender.EnqueueMessage($"{finalWinner.NameWithWorld} is the final winner with {(finalWinner.FinalScore.HasValue ? finalWinner.FinalScore.Value.ToString("N0") : "No Score")}. They win the pot totalling {totalPot:N0}! Trading {finalWinner.NameWithWorld} Now!");
                }
                else
                {
                    Notify.Error("No final winner.");
                }
            }
        }
    }
}
