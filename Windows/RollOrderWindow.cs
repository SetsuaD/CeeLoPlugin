using CeeLoPlugin.Models;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Collections.Generic;

namespace CeeLoPlugin.Windows
{
    public class RollOrderWindow : Window
    {
        private readonly IChatGui _chatGui; // Declare ChatGui as a field

        public List<Player> Players { get; private set; } = new(); // List of players

        // Updated constructor to accept IChatGui
        public RollOrderWindow(IChatGui chatGui) : base("Roll Order")
        {
            _chatGui = chatGui; // Initialize it
            // Other initialization code...
        }

        // Override the inherited Draw method to provide custom rendering
        public override void Draw()
        {
            // Example UI elements
            ImGui.Text("Roll Order Window");

            // Draw players list
            foreach (var player in Players)
            {
                ImGui.Text($"Player: {player.Name}");
                // Display the player's roll or some other data you need to show
            }

            // Button to announce the roll order
            if (ImGui.Button("Send Roll Order"))
            {
                // Example of sending a roll order message to the chat
                _chatGui.Print("/shout The roll order has been decided!");
            }

            // Button to announce scores or other game-related messages
            if (ImGui.Button("Announce Scores"))
            {
                _chatGui.Print("/shout Player1 has 5 points! New lead score!");
            }
        }
    }
}
