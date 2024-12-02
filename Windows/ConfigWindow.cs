using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CeeLoPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration _configuration;

    public ConfigWindow(Plugin plugin) : base("CeeLo Configuration###CeeLoConfig")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(320, 180);
        SizeCondition = ImGuiCond.FirstUseEver;

        _configuration = plugin.Configuration;
    }

    public void Dispose()
    {
        // Any cleanup logic if needed
    }

    public override void PreDraw()
    {
        // Adjust movability dynamically based on configuration
        if (_configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        ImGui.Text("CeeLo Configuration Settings");

        ImGui.Separator();

        // Feature toggle
        var enableFeature = _configuration.EnableFeature;
        if (ImGui.Checkbox("Enable CeeLo Feature", ref enableFeature))
        {
            _configuration.EnableFeature = enableFeature;
            _configuration.Save();
        }

        // Max players slider
        var maxPlayers = _configuration.MaxPlayers;
        if (ImGui.SliderInt("Max Players", ref maxPlayers, 2, 20))
        {
            _configuration.MaxPlayers = maxPlayers;
            _configuration.Save();
        }

        // Movable window checkbox
        var movable = _configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            _configuration.IsConfigWindowMovable = movable;
            _configuration.Save();
        }

        ImGui.Separator();

        // Reset settings button
        if (ImGui.Button("Reset to Default"))
        {
            _configuration.ResetToDefaults();
            _configuration.Save();
        }

        ImGui.SameLine();

        // Close window button
        if (ImGui.Button("Close"))
        {
            this.IsOpen = false;
        }
    }
}
