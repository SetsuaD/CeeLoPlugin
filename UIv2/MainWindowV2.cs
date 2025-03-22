using System;
using System.IO;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.ImGuiMethods;
using ImGuiNET;
using static CeeLoPlugin.Plugin;
using CeeLoPlugin.UIv2.DataStructures; // For PlayerData

namespace CeeLoPlugin.UIv2;
public class MainWindowV2 : Window
{
    private FileDialogManager FileDialogManager = new();
    public MainWindowV2() : base($"{Svc.PluginInterface.Manifest.Name} configuration")
    {
        Instance.WindowSystem.AddWindow(this);
        this.SetMinSize();
    }

    public override void Draw()
    {
        FileDialogManager.Draw();
        if (ImGui.BeginTabBar("##defaultTabBar"))
        {
            if (ImGui.BeginTabItem("Configuration"))
            {
                DrawConfiguration();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Game (Roll Recording)"))
            {
                DrawGameRolls();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Log"))
            {
                DrawLog();
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
    }

    private void DrawConfiguration()
    {
        // Communication Mode Settings
        ImGuiEx.Text($"Communication Mode Settings:");
        ImGui.Indent();
        ImGui.SetNextItemWidth(200f);
        ImGuiEx.EnumCombo("Chat Channel", ref Instance.Configuration.ChatChannel, filter: Utils.AllowedChatTypes.ContainsKey);
        ImGui.Unindent();

        // Game Parameters
        ImGuiEx.Text($"Game Parameters:");
        ImGui.Indent();

        ImGui.SetNextItemWidth(200f);
        ImGuiEx.InputFancyNumeric("GIL Bet", ref Instance.Configuration.GilBet.ValidateRange(1000, 100_000_000), 0);

        ImGui.SetNextItemWidth(200f);
        if (ImGui.InputFloat("House Cut (%)", ref Instance.Configuration.HouseCut.ValidateRange(0f, 50f), 0.1f, 1f, "%.0f%%"))
        {
            Instance.Configuration.Save();
        }
        ImGuiEx.Text("Log Folder Path Override:");
        ImGui.Indent();
        if (ImGuiEx.IconButton(FontAwesomeIcon.Folder))
        {
            FileDialogManager.OpenFolderDialog("Select Log Folder", (success, path) =>
            {
                if (success)
                {
                    Instance.Configuration.LogPath = path;
                }
            });
        }
        if (Instance.Configuration.LogPath != "")
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.FolderMinus))
            {
                Instance.Configuration.LogPath = "";
            }
        }
        ImGui.SameLine();
        ImGuiEx.Text(Instance.Configuration.LogPath == "" ? "Default" : Instance.Configuration.LogPath);
        ImGui.Unindent();
        ImGui.Unindent();

        ImGui.Separator();

        // >> Removed the player management buttons from here (they are now in the Game (Roll Recording) tab) <<
    }

    private void DrawGameRolls()
    {
        // --- Row 1: Advertise (moved above player management) ---
        ImGuiEx.LineCentered("Advertise", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Comment, "Advertise"))
            {
                Instance.ChatSender.EnqueueMessage("Hello to new players joining us! The game is called Ceelo. Players compete against each other as a group, with the winner taking the pot!");
                Instance.ChatSender.EnqueueMessage("Join at the start of a round by trading the dealer the current bet. Then /random 99 to get roll order. Then /random 6 three times when its your turn to score!");
                Instance.ChatSender.EnqueueMessage("See complete rules at: setsunai.settzyvents.com and join our discord at: https://discord.gg/uSG7Y2RtZ7");
            }
        });

        // --- Row 2: Player Management Buttons ---
        ImGuiEx.LineCentered("Player Management", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Crosshairs, "Add Target", Svc.Targets.Target is IPlayerCharacter))
            {
                var pc = (IPlayerCharacter)Svc.Targets.Target!;
                if (!Instance.Configuration.PlayerDatas.Any(x => x.NameWithWorld == pc.GetNameWithWorld()))
                {
                    Instance.Configuration.PlayerDatas.Add(new PlayerData()
                    {
                        NameWithWorld = pc.GetNameWithWorld()
                        // Note: FinalScore remains null until input.
                    });
                }
                else
                {
                    Notify.Error("This player already added!");
                }
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Sort, "Sort"))
            {
                Instance.Configuration.PlayerDatas = Instance.Configuration.PlayerDatas
                    .OrderByDescending(x => x.OrderDeterminingRolls != null ? string.Join("", x.OrderDeterminingRolls.Select(s => s.ToString("D3"))) : "")
                    .ToList();
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.User, "Add Fake"))
            {
                var name1 = "EYUIUOA";
                var name2 = "WRTPSDFGHJKLZXCVBNM";
                var name = "";
                for (var i = 0; i < 16; i++)
                {
                    var s = i % 2 == 0 ? name1.GetRandom().ToString() : name2.GetRandom().ToString();
                    if (i % 6 != 0) s = s.ToLower();
                    if (i == 5) s += " ";
                    if (i == 11) s += "@";
                    name += s;
                }
                Instance.Configuration.PlayerDatas.Add(new PlayerData()
                {
                    NameWithWorld = name
                });
            }
        });

        // --- Row 3: Player Table ---
        // Updated table header order: Name, Bet Confirm, Roll Order, Rolls, Final Score, Control.
        if (ImGuiEx.BeginDefaultTable(new string[] { "~Name", "Bet Confirm", "Roll Order", "Rolls", "Final Score", "##control" }))
        {
            foreach (var player in Instance.Configuration.PlayerDatas)
            {
                ImGui.PushID(player.ID);
                ImGui.TableNextRow();

                // Column 1: Player Name with removal icon.
                ImGui.TableNextColumn();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                {
                    Svc.Framework.RunOnTick(() => Instance.Configuration.PlayerDatas.Remove(player));
                }
                ImGui.SameLine();
                ImGuiEx.TextV(player.NameWithWorld);

                // Column 2: Bet Confirm Checkbox.
                ImGui.TableNextColumn();
                ImGui.Checkbox($"##betConfirm{player.ID}", ref player.BetCollected);

                // Column 3: Roll Order (editable list without add/remove icons)
                ImGui.TableNextColumn();
                for (var i = 0; i < player.OrderDeterminingRolls.Count; i++)
                {
                    if (i > 0)
                        ImGui.SameLine(0, 1);
                    ImGui.PushID($"order{i}");
                    ImGui.SetNextItemWidth(50f);
                    var orderVal = player.OrderDeterminingRolls[i];
                    if (ImGui.InputInt("##order", ref orderVal, 0, 0))
                    {
                        player.OrderDeterminingRolls[i] = orderVal;
                    }
                    ImGui.PopID();
                }

                // Column 4: Game Rolls (3 roll fields)
                ImGui.TableNextColumn();
                for (var i = 0; i < player.GameRolls.Length; i++)
                {
                    if (i > 0) ImGui.SameLine(0, 1);
                    ImGui.PushID($"roll{i}");
                    ImGui.SetNextItemWidth(50);
                    ImGui.InputInt("##inputRoll", ref player.GameRolls[i], 0);
                    ImGui.PopID();
                }

                // Column 5: Final Score (editable via InputText)
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(50);
                // Using a unique label for the input field.
                if (ImGui.InputText($"##finalScore{player.ID}", ref player.FinalScoreInput, 16))
                {
                    // If the input is empty, set FinalScore to null.
                    if (string.IsNullOrWhiteSpace(player.FinalScoreInput))
                    {
                        player.FinalScore = null;
                    }
                    else if (int.TryParse(player.FinalScoreInput, out int parsed))
                    {
                        player.FinalScore = parsed;
                    }
                    else
                    {
                        // If parsing fails, reset FinalScore to null.
                        player.FinalScore = null;
                    }
                }

                // Column 6: Winner control (Crown button)
                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Crown.ToIconString(), ref player.IsWinner, EColor.Green);
                ImGui.PopFont();
                ImGui.PopID();
            }
            ImGui.EndTable();
        }

        // --- Row 4: Dealer Announcements (Round Start) ---
        ImGuiEx.LineCentered("Dealer Announcements (Round Start)", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Bullhorn, "Announce New Round"))
            {
                Instance.ChatSender.EnqueueMessage("New Round: 100k to 100m. At least 3 players must agree on the bet to set the round bet. What shall this round's bet be?");
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.DollarSign, "Set Round Bet"))
            {
                Instance.ChatSender.EnqueueMessage($"This round's bet is {Instance.Configuration.GilBet:N0}. Place your bets!");
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.MoneyBillWave, "Announce Pot Total"))
            {
                int baseTotal = Instance.Configuration.GilBet * Instance.Configuration.PlayerDatas.Count;
                int houseCutAmount = (int)(baseTotal * (Instance.Configuration.HouseCut / 100f));
                int totalPot = baseTotal - houseCutAmount;
                Instance.ChatSender.EnqueueMessage($"Current pot total is {totalPot:N0}.");
            }
            ImGui.SameLine();
            // Updated Announce Last Chance button with bet confirmation details.
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Clock, "Announce Last Chance"))
            {
                var notConfirmed = Instance.Configuration.PlayerDatas.Where(p => !p.BetCollected).Select(p => p.NameWithWorld);
                if (notConfirmed.Any())
                {
                    string notConfirmedList = string.Join(", ", notConfirmed);
                    Instance.ChatSender.EnqueueMessage($"Last chance to place your bet. Closing Soon™! Waiting on: {notConfirmedList} for bet trade still.");
                }
                else
                {
                    Instance.ChatSender.EnqueueMessage("Last chance to place your bet. Closing Soon™!");
                }
            }
            ImGui.SameLine();
            // --- Updated Bets Closed Button ---
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Stop, "Announce Bets Closed"))
            {
                int baseTotal = Instance.Configuration.GilBet * Instance.Configuration.PlayerDatas.Count;
                int houseCutAmount = (int)(baseTotal * (Instance.Configuration.HouseCut / 100f));
                int totalPot = baseTotal - houseCutAmount;
                Instance.ChatSender.EnqueueMessage($"Bets CLOSED. Current Pot Total is: {totalPot:N0}. Moving to Roll Order Phase! (/random 99)");
            }
        });

        // --- Row 5: Dealer Announcements (Roll Order & Turns) ---
        ImGuiEx.LineCentered("Dealer Announcements (Roll Order & Turns)", () =>
        {
            // New: Recall For RollOrder (moved to far left)
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Retweet, "Recall For RollOrder"))
            {
                var missingRollOrderPlayers = Instance.Configuration.PlayerDatas
                    .Where(p => p.OrderDeterminingRolls.Count == 0 || p.OrderDeterminingRolls[0] == 0)
                    .Select(p => p.NameWithWorld);
                if (missingRollOrderPlayers.Any())
                {
                    string names = string.Join(", ", missingRollOrderPlayers);
                    Instance.ChatSender.EnqueueMessage($"Waiting on Roll Order Roll for: {names}. Please /random 99 for Roll Order.");
                }
            }
            ImGui.SameLine();
            // New: RollOrderTie button with updated message
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.ExclamationTriangle, "RollOrderTie"))
            {
                var tiedGroups = Instance.Configuration.PlayerDatas
                    .Where(p => p.OrderDeterminingRolls.Count > 0)
                    .GroupBy(p => p.OrderDeterminingRolls[0])
                    .Where(g => g.Count() > 1 && g.Key != 0);
                if (tiedGroups.Any())
                {
                    var tiedPlayers = tiedGroups.SelectMany(g => g).Select(p => p.NameWithWorld).Distinct();
                    string tiedList = tiedPlayers.Any() ? string.Join(", ", tiedPlayers) : "None";
                    Instance.ChatSender.EnqueueMessage($"Players {tiedList} please reroll. (/random 99)");
                }
            }
            ImGui.SameLine();
            // Existing: Announce Roll Order (auto-sorts players)
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.List, "Announce Roll Order"))
            {
                // Auto-sort players by their roll order
                Instance.Configuration.PlayerDatas = Instance.Configuration.PlayerDatas
                    .OrderByDescending(x => x.OrderDeterminingRolls != null ? string.Join("", x.OrderDeterminingRolls.Select(s => s.ToString("D3"))) : "")
                    .ToList();
                var rollOrder = string.Join(", ", Instance.Configuration.PlayerDatas.Select(x => x.NameWithWorld));
                Instance.ChatSender.EnqueueMessage($"Roll order sorted! Current roll order is: {rollOrder}.");
            }
            ImGui.SameLine();
            // Updated: Call Next Player uses only players with FinalScore == null.
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.UserCheck, "Call Next Player"))
            {
                var nextPlayer = Instance.Configuration.PlayerDatas.FirstOrDefault(x => x.FinalScore == null);
                if (nextPlayer != null)
                {
                    var recentPlayer = Instance.Configuration.PlayerDatas.LastOrDefault(x => x.FinalScore.HasValue);
                    string recentText = recentPlayer != null
                        ? $"{recentPlayer.NameWithWorld} has scored a {recentPlayer.FinalScore.Value}. "
                        : "";
                    Instance.ChatSender.EnqueueMessage($"{recentText}{nextPlayer.NameWithWorld}, Roll your dice! (/random 6 three times)");
                }
                else
                {
                    Notify.Error("All players have rolled.");
                }
            }
            ImGui.SameLine();
            // Updated: Roll Again uses only players with FinalScore == null.
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Redo, "Roll Again"))
            {
                var nextPlayer = Instance.Configuration.PlayerDatas.FirstOrDefault(x => x.FinalScore == null);
                if (nextPlayer != null)
                {
                    Instance.ChatSender.EnqueueMessage($"{nextPlayer.NameWithWorld}, No score. Roll again. (/random 6 three times)");
                }
                else
                {
                    Notify.Error("All players have rolled.");
                }
            }
        });

        // --- NEW Row: Tie Breaker Options ---
        ImGuiEx.LineCentered("Tie Breaker Options", () =>
        {
            // Multi-Tie Button: For 3 or more players tying.
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Users, "Multi-Tie"))
            {
                int maxScore = Instance.Configuration.PlayerDatas.Max(p => p.FinalScore ?? 0);
                var multiTieGroup = Instance.Configuration.PlayerDatas
                    .Where(p => (p.FinalScore ?? 0) == maxScore)
                    .Where(p => p.FinalScore.HasValue)
                    .ToList();
                if (multiTieGroup.Count >= 3)
                {
                    var names = string.Join(", ", multiTieGroup.Select(p => p.NameWithWorld));
                    Instance.ChatSender.EnqueueMessage($"Game has ended in a multi-tie. These players, {names} will re-roll order and re-score to win the pot. Roll for order {names}! (/random 99)");
                }
                else
                {
                    Notify.Error("No multi-tie detected.");
                }
            }
            ImGui.SameLine();
            // 2-Player Tie (Roulette) Button.
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Dice, "Roulette"))
            {
                int maxScore = Instance.Configuration.PlayerDatas.Max(p => p.FinalScore ?? 0);
                var twoTieGroup = Instance.Configuration.PlayerDatas.Where(p => (p.FinalScore ?? 0) == maxScore).ToList();
                if (twoTieGroup.Count == 2)
                {
                    // Remove players not in the two-tie group
                    Instance.Configuration.PlayerDatas = twoTieGroup.ToList();
                    // Clear final scores and crown states for tie-breaker preparation
                    foreach (var player in Instance.Configuration.PlayerDatas)
                    {
                        player.FinalScore = null;
                        player.FinalScoreInput = "";
                        player.IsWinner = false;
                    }
                    string names = string.Join(", ", twoTieGroup.Select(p => p.NameWithWorld));
                    Instance.ChatSender.EnqueueMessage($"{names} tied. Initiating roulette tie-breaker round. Roll for order (/random 99) and high roll goes first. You will roll (/random 6) and the first to score a 1 loses. The next player will roll (/random 5) and it counts down each roll. Winner takes all!");
                }
                else
                {
                    Notify.Error("No two-player tie detected for roulette.");
                }
            }
        });

        // --- Row 6: Winner Announcement ---
        ImGuiEx.LineCentered("Winner Announcement", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Crown, "Announce Current Winner"))
            {
                int maxScore = Instance.Configuration.PlayerDatas.Max(p => p.FinalScore ?? 0);
                var tiedPlayers = Instance.Configuration.PlayerDatas.Where(p => (p.FinalScore ?? 0) == maxScore).ToList();
                if (tiedPlayers.Count > 1)
                {
                    bool allCrowned = tiedPlayers.All(p => p.IsWinner);
                    string names = string.Join(", ", tiedPlayers.Select(p => p.NameWithWorld));
                    if (allCrowned)
                    {
                        Instance.ChatSender.EnqueueMessage($"Tie detected: {names} have the same top score of {maxScore:N0} and have all marked their crowns. They will split the winnings.");
                    }
                    else
                    {
                        Instance.ChatSender.EnqueueMessage($"Tie detected: {names} have the same top score of {maxScore:N0}. Please decide to either split the winnings or choose a roulette tie-breaker.");
                    }
                }
                else if (tiedPlayers.Count == 1)
                {
                    var winner = tiedPlayers.First();
                    Instance.ChatSender.EnqueueMessage($"{winner.NameWithWorld} is the highest score with {winner.FinalScore?.ToString("N0") ?? "No Score"}.");
                }
                else
                {
                    Notify.Error("No current winner.");
                }
            }
        });

        // --- Row 7: Final Round Announcement ---
        ImGuiEx.LineCentered("Final Round Announcement", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.FlagCheckered, "Announce Final Winner"))
            {
                // If only two players remain, assume roulette mode.
                if (Instance.Configuration.PlayerDatas.Count == 2)
                {
                    var players = Instance.Configuration.PlayerDatas;
                    bool p1Crown = players[0].IsWinner;
                    bool p2Crown = players[1].IsWinner;
                    if (p1Crown && !p2Crown)
                    {
                        Instance.ChatSender.EnqueueMessage($"{players[0].NameWithWorld} wins the roulette tie-breaker!");
                    }
                    else if (!p1Crown && p2Crown)
                    {
                        Instance.ChatSender.EnqueueMessage($"{players[1].NameWithWorld} wins the roulette tie-breaker!");
                    }
                    else
                    {
                        Notify.Error("Ambiguous crown selection. Please resolve tie-breaker by ensuring only one crown is selected.");
                    }
                }
                else
                {
                    var finalWinner = Instance.Configuration.PlayerDatas.OrderByDescending(x => x.FinalScore ?? 0).FirstOrDefault();
                    if (finalWinner != null)
                    {
                        int baseTotal = Instance.Configuration.GilBet * Instance.Configuration.PlayerDatas.Count;
                        int houseCutAmount = (int)(baseTotal * (Instance.Configuration.HouseCut / 100f));
                        int totalPot = baseTotal - houseCutAmount;
                        Instance.ChatSender.EnqueueMessage($"{finalWinner.NameWithWorld} is the final winner with {(finalWinner.FinalScore.HasValue ? finalWinner.FinalScore.Value.ToString("N0") : "No Score")}. They win the pot totalling {totalPot:N0}! Trading {finalWinner.NameWithWorld} Now!");
                    }
                    else
                    {
                        Notify.Error("No final winner.");
                    }
                }
            }
        });

        // --- Row 8: Game Controls ---
        ImGuiEx.LineCentered("Game Controls", () =>
        {
            // Calculate current pot total
            int baseTotal = Instance.Configuration.GilBet * Instance.Configuration.PlayerDatas.Count;
            int houseCutAmount = (int)(baseTotal * (Instance.Configuration.HouseCut / 100f));
            int totalPot = baseTotal - houseCutAmount;

            // Initialize trade gil value if not already done
            if (!TradeGilState.Initialized)
            {
                TradeGilState.TradeGilValue = totalPot;
                TradeGilState.Initialized = true;
            }
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##TradeGil", ref TradeGilState.TradeGilValue, 0);
            ImGui.SameLine();
            // Updated: Using HandHoldingUsd icon in place of Exchange.
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.HandHoldingUsd, "Trade Gil / Focus Target"))
            {
                if (Svc.Targets.Target is IPlayerCharacter)
                {
                    Notify.Info($"Focusing target: {(Svc.Targets.Target as IPlayerCharacter)?.GetNameWithWorld()}");
                    Notify.Info($"Initiating trade for {TradeGilState.TradeGilValue:N0} gil.");
                }
                else
                {
                    Notify.Error("No valid target selected for trade.");
                }
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Undo, "Start Over Button"))
            {
                Instance.Configuration.PlayerDatas.Clear();
                Notify.Info("Player table cleared. Game restarted.");
            }
        });
    }

    private void DrawLog()
    {
        ImGuiEx.LineCentered(() =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Save, "Save Log"))
            {
                string defaultFolder = string.IsNullOrEmpty(Instance.Configuration.LogPath)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "FINAL FANTASY XIV - A Realm Reborn")
                    : Instance.Configuration.LogPath;

                if (!Directory.Exists(defaultFolder))
                {
                    Directory.CreateDirectory(defaultFolder);
                }

                FileDialogManager.SaveFileDialog("Save log as...", defaultFolder, "ceelochatlog", "txt", (success, path) =>
                {
                    if (success)
                    {
                        if (Path.GetExtension(path) != ".txt")
                        {
                            path += ".txt";
                        }

                        if (!File.Exists(path))
                        {
                            File.WriteAllText(path, Instance.Configuration.Log.Join("\n"));
                            Instance.Configuration.Log.Clear();
                        }
                        else
                        {
                            Notify.Error($"File already exists:\n{path}");
                        }
                    }
                });
            }

            ImGui.SameLine();
            if (ImGui.Button("Add fake lines"))
            {
                Instance.Configuration.Log.AddRange("""
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit.
                    Donec congue vulputate lectus nec facilisis.
                    Nulla facilisi.
                    Aenean vitae augue neque.
                    Cras imperdiet justo vel erat aliquet, sed condimentum dolor rutrum.
                    Cras molestie volutpat pretium.
                    Sed sit amet dolor dapibus, viverra nisi vitae, elementum quam.
                    Donec sed quam lectus.
                    Phasellus faucibus turpis nec erat semper, sed blandit odio tempus.
                    Mauris pretium leo quis ligula elementum molestie.
                    Donec aliquet quis metus eu mattis.
                    Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Sed id tellus porta nunc aliquet pharetra ut a sem.
                    Aliquam auctor ante placerat arcu placerat sagittis.
                    Vestibulum finibus efficitur lorem a laoreet.
                    Suspendisse vulputate luctus justo vitae pulvinar.
                    """.Split("\n"));
            }
        });

        ImGui.Separator();
        ImGuiEx.Text("Log Folder Path Override:");
        ImGui.Indent();
        if (ImGuiEx.IconButton(FontAwesomeIcon.Folder))
        {
            FileDialogManager.OpenFolderDialog("Select Log Folder", (success, path) =>
            {
                if (success)
                {
                    Instance.Configuration.LogPath = path;
                    Instance.Configuration.Save();
                }
            });
        }

        if (!string.IsNullOrEmpty(Instance.Configuration.LogPath))
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.FolderMinus))
            {
                Instance.Configuration.LogPath = "";
                Instance.Configuration.Save();
            }
        }

        ImGui.SameLine();
        ImGuiEx.Text(string.IsNullOrEmpty(Instance.Configuration.LogPath) ? "Default" : Instance.Configuration.LogPath);
        ImGui.Unindent();

        if (ImGuiEx.BeginDefaultTable(new string[] { "~LogLine" }, false))
        {
            foreach (var line in Instance.Configuration.Log)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.TextWrapped(line);
            }
            ImGui.EndTable();
        }
    }
}

// Helper static class to store trade gil state.
public static class TradeGilState
{
    public static bool Initialized = false;
    public static int TradeGilValue = 0;
}
