using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CeeLoPlugin.Windows;

public class MainWindow : Window
{
    private readonly Plugin _plugin; // Reference to the main plugin instance

    public MainWindow(Plugin plugin) : base("CeeLo Plugin")
    {
        _plugin = plugin; // Store the plugin instance for future use

        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;
        Size = new System.Numerics.Vector2(400, 300);
    }

    public override void Draw()
    {
        ImGui.Text("Welcome to CeeLo!");

        if (ImGui.Button("Start Game"))
        {
            ImGui.Text("Game Starting...");
            _plugin.GameLogic?.StartGame();
        }

        if (ImGui.Button("Rules"))
        {
            ImGui.OpenPopup("RulesPopup");
        }

        if (ImGui.BeginPopup("RulesPopup"))
        {
            ImGui.Text("CeeLo is a dice game. Roll for a win!");
            ImGui.EndPopup();
        }

        if (ImGui.Button("Configuration"))
        {
            _plugin.OpenConfigUI();
        }
    }
}
