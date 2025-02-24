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
                        NameWithWorld = pc.GetNameWithWorld(),
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
                    .OrderByDescending(x => string.Join("", x.OrderDeterminingRolls.Select(s => s.ToString("D3"))))
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
        if (ImGuiEx.BeginDefaultTable(new string[] { "~Name", "Roll Order", "Rolls", "Final Score", "##control" }))
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

                // Column 2: Roll Order (editable list without add/remove icons)
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

                // Column 3: Game Rolls (3 roll fields)
                ImGui.TableNextColumn();
                for (var i = 0; i < player.GameRolls.Length; i++)
                {
                    if (i > 0) ImGui.SameLine(0, 1);
                    ImGui.PushID($"roll{i}");
                    ImGui.SetNextItemWidth(50);
                    ImGui.InputInt("##inputRoll", ref player.GameRolls[i], 0);
                    ImGui.PopID();
                }

                // Column 4: Final Score (editable)
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(50);
                ImGui.InputInt("##finalScore", ref player.FinalScore, 0);

                // Column 5: Winner control (Crown button)
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
                Instance.ChatSender.EnqueueMessage("New Round: 100k to 100m. What shall this round's bet be?");
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
                Instance.ChatSender.EnqueueMessage($"Current pot total is {totalPot:N0}. Betting closing soon™!");
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Clock, "Announce Last Chance"))
            {
                Instance.ChatSender.EnqueueMessage("Last chance to place your bet. Closing in 5 seconds.");
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Stop, "Announce Bets Closed"))
            {
                Instance.ChatSender.EnqueueMessage("Bets Closed. Roll for order! (/random 99)");
            }
        });

        // --- Row 5: Dealer Announcements (Roll Order & Turns) ---
        ImGuiEx.LineCentered("Dealer Announcements (Roll Order & Turns)", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.List, "Announce Roll Order"))
            {
                var rollOrder = string.Join(", ", Instance.Configuration.PlayerDatas
                    .OrderByDescending(x => string.Join("", x.OrderDeterminingRolls.Select(s => s.ToString("D3"))))
                    .Select(x => x.NameWithWorld));
                Instance.ChatSender.EnqueueMessage($"Roll order sorted! Current roll order is: {rollOrder}.");
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.UserCheck, "Call Next Player"))
            {
                var nextPlayer = Instance.Configuration.PlayerDatas.FirstOrDefault(x => x.FinalScore <= 0);
                if (nextPlayer != null)
                {
                    Instance.ChatSender.EnqueueMessage($"{nextPlayer.NameWithWorld}, Roll your dice! (/random 6 three times)");
                }
                else
                {
                    Notify.Error("All players have rolled.");
                }
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Redo, "Roll Again"))
            {
                var nextPlayer = Instance.Configuration.PlayerDatas.FirstOrDefault(x => x.FinalScore <= 0);
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

        // --- Row 6: Winner Announcement ---
        ImGuiEx.LineCentered("Winner Announcement", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Crown, "Announce Current Winner"))
            {
                var currentWinner = Instance.Configuration.PlayerDatas.OrderByDescending(x => x.FinalScore).FirstOrDefault();
                if (currentWinner != null)
                {
                    Instance.ChatSender.EnqueueMessage($"{currentWinner.NameWithWorld} is the highest score with {currentWinner.FinalScore:N0}.");
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
                var finalWinner = Instance.Configuration.PlayerDatas.OrderByDescending(x => x.FinalScore).FirstOrDefault();
                if (finalWinner != null)
                {
                    Instance.ChatSender.EnqueueMessage($"{finalWinner.NameWithWorld} is the final winner with {finalWinner.FinalScore:N0}.");
                }
                else
                {
                    Notify.Error("No final winner.");
                }
            }
        });

        // --- Row 8: Game Controls ---
        ImGuiEx.LineCentered("Game Controls", () =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Play, "Start Game Button"))
            {
                // logic to start game
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Undo, "Start Over Button"))
            {
                // Clear the player table (keeping the bet and house cut values)
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
                // Use override path if set; otherwise, default to FFXIV Documents folder.
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
