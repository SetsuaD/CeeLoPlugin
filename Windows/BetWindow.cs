using Dalamud.Game.Gui; // Add this import
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace CeeLoPlugin.Windows
{
    public class BetWindow : Window
    {
        private readonly IChatGui _chatGui; // Declare ChatGui as a member variable

        // Updated constructor that accepts IChatGui
        public BetWindow(IChatGui chatGui) : base("Place Your Bets")
        {
            Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;
            Size = new System.Numerics.Vector2(400, 300);

            // Set ChatGui from the argument
            _chatGui = chatGui;
        }

        public override void Draw()
        {
            // Example UI for placing bets
            ImGui.Text("Step 1 - Agree on the Bet");

            var betAmount1 = "200k";
            var betAmount2 = "1M";
            var betAmount3 = "10M";

            if (ImGui.Button("Send Bet Summary"))
            {
                // Send the bet summary via chat using the _chatGui instance
                _chatGui.Print($"/shout Current bets: {betAmount1}, {betAmount2}, {betAmount3}");
            }

            // More UI elements for betting can be added here
        }
    }
}
