using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CeeLoPlugin;

public class MainWindow : Window
{
    public MainWindow() : base("CeeLo Plugin")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;
        Size = new System.Numerics.Vector2(400, 300);
    }

    public override void Draw()
    {
        ImGui.Text("Welcome to CeeLo!");

        if (ImGui.Button("Start Game"))
        {
            ImGui.Text("Game Starting...");
            // Game logic can be added here.
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
    }
}
