using CeeLoPlugin;
using Dalamud.Configuration;

public class Configuration : IPluginConfiguration
{
    // Required property for IPluginConfiguration
    public int Version { get; set; } = 0;

    // Enables or disables the core feature of the CeeLo plugin.
    public bool EnableFeature { get; set; } = true;

    // Configurable maximum number of players in the game.
    public int MaxPlayers { get; set; } = 20;

    // Allows the configuration window to be moved dynamically.
    public bool IsConfigWindowMovable { get; set; } = true;

    // Saves the current configuration using the Dalamud Plugin Interface.
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    // Resets all configurable properties to their default values.
    public void ResetToDefaults()
    {
        EnableFeature = true;
        MaxPlayers = 8;
        IsConfigWindowMovable = true;
    }
}
