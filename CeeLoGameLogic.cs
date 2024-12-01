using System;
using System.Collections.Generic;
using CeeLoPlugin.Models;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;

namespace CeeLoPlugin.Logic
{
    public class CeeLoGameLogic
    {
        public List<Player> Players { get; private set; } = new();

        public void AddPlayer()
        {
            // Get the target object from ObjectTable and verify it's a valid player
            foreach (var gameObject in Services.ObjectTable)
            {
                if (gameObject.ObjectKind == ObjectKind.Player && gameObject is IPlayerCharacter player)
                {
                    var playerName = player.Name.TextValue;

                    // Check if the player is already in the game
                    if (Players.Exists(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                    {
                        Services.ChatGui.PrintError($"{playerName} is already in the game.");
                        return;
                    }

                    // Add the player to the game
                    Players.Add(new Player(playerName));
                    Services.ChatGui.Print($"{playerName} has been added to the game.");
                    return;
                }
            }

            // If no valid player was found in the target
            Services.ChatGui.PrintError("No valid player found as target.");
        }

        public void StartGame()
        {
            if (Players.Count == 0)
            {
                Services.ChatGui.PrintError("No players to start the game.");
                return;
            }

            Services.ChatGui.Print("The game has started! Players, please roll for turn order using /random.");
        }

        public void ResetGame()
        {
            Players.Clear();
            Services.ChatGui.Print("The game has been reset.");
        }
    }
}
