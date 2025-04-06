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
using CeeLoPlugin.UIv2.DataStructures;
using CeeLoPlugin.Logic; // Reference to RollActions

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
        ImGuiEx.Text("Communication Mode Settings:");
        ImGui.Indent();
        ImGui.SetNextItemWidth(200f);
        ImGuiEx.EnumCombo("Chat Channel", ref Instance.Configuration.ChatChannel, filter: Utils.AllowedChatTypes.ContainsKey);
        ImGui.Unindent();

        // Game Parameters
        ImGuiEx.Text("Game Parameters:");
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
        // --- Row 1: Advertise ---
        ImGuiEx.LineCentered("Advertise", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Comment, "Advertise"))
            {
                Instance.ChatSender.EnqueueMessage("Hello to new players joining us! The game is called Ceelo. Players compete against each other as a group, with the winner taking the pot!");
                Instance.ChatSender.EnqueueMessage("Join at the start of a round by trading the dealer the current bet. Then /random 99 to get roll order. Then /random 6 three times when it's your turn to score!");
                Instance.ChatSender.EnqueueMessage("See complete rules at: setsunai.settzyvents.com and join our discord at: https://discord.gg/uSG7Y2RtZ7");
                Instance.ChatSender.EnqueueMessage("Note: Hard 456 Instant Wins. Hard 123 Instant Loses. Soft 123(213/etc) is a no score. Soft 456(546/etc) is >1-6 but <111-666 Triples Scores.");
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
                        // FinalScore remains null until input.
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
                    .OrderByDescending(x => x.OrderDeterminingRolls != null
                        ? string.Join("", x.OrderDeterminingRolls.Select(s => s.ToString("D3")))
                        : "")
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

                // Column 3: Roll Order (editable list)
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
                if (ImGui.InputText($"##finalScore{player.ID}", ref player.FinalScoreInput, 16))
                {
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
                RollActions.AnnounceNewRound();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.DollarSign, "Set Round Bet"))
                RollActions.SetRoundBet();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.MoneyBillWave, "Announce Pot Total"))
                RollActions.AnnouncePotTotal();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Clock, "Announce Last Chance"))
                RollActions.AnnounceLastChance();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Stop, "Announce Bets Closed"))
                RollActions.AnnounceBetsClosed();
        });

        // --- Row 5: Dealer Announcements (Roll Order & Turns) ---
        ImGuiEx.LineCentered("Dealer Announcements (Roll Order & Turns)", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Retweet, "Recall For RollOrder"))
                RollActions.RecallForRollOrder();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.ExclamationTriangle, "RollOrderTie"))
                RollActions.RollOrderTie();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.List, "Announce Roll Order"))
                RollActions.AnnounceRollOrder();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.UserCheck, "Call Next Player"))
                RollActions.CallNextPlayer();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Redo, "Roll Again"))
                RollActions.RollAgain();
        });

        // --- New Row: Tie Resolution ---
        ImGuiEx.LineCentered("Tie Resolution", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Handshake, "Announce Tie"))
                RollActions.ResolveTie();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.CheckDouble, "Split Tie"))
                RollActions.ResolveTieSplit();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Dice, "Roulette Tie"))
                RollActions.ResolveTieRoulette();
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Users, "Multi Tie"))
                RollActions.ResolveMultiTie();
        });

        // --- Row 6: Winner Announcement ---
        ImGuiEx.LineCentered("Winner Announcement", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Crown, "Announce Current Winner"))
                RollActions.AnnounceCurrentWinner();
        });

        // --- Row 7: Final Round Announcement ---
        ImGuiEx.LineCentered("Final Round Announcement", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.FlagCheckered, "Announce Final Winner"))
                RollActions.AnnounceFinalWinner();
        });


        // --- Row 8: Game Controls ---
        ImGuiEx.LineCentered("Game Controls", () =>
        {
            int baseTotal = Instance.Configuration.GilBet * Instance.Configuration.PlayerDatas.Count;
            int houseCutAmount = (int)(baseTotal * (Instance.Configuration.HouseCut / 100f));
            int totalPot = baseTotal - houseCutAmount;

            if (!TradeGilState.Initialized)
            {
                TradeGilState.TradeGilValue = totalPot;
                TradeGilState.Initialized = true;
            }
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##TradeGil", ref TradeGilState.TradeGilValue, 0);
            ImGui.SameLine();
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
                Instance.Configuration.GilBet = 1_000_000; // Reset bet to 1,000,000 gil
                Instance.Configuration.Save();          // Save configuration changes
                Notify.Info("Player table cleared. Game restarted. Bet reset to 1,000,000 gil.");
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
                    Directory.CreateDirectory(defaultFolder);

                FileDialogManager.SaveFileDialog("Save log as...", defaultFolder, "ceelochatlog", "txt", (success, path) =>
                {
                    if (success)
                    {
                        if (Path.GetExtension(path) != ".txt")
                            path += ".txt";

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
