using CeeLoPlugin;
using CeeLoPlugin.UIv2.DataStructures;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using ECommons.DalamudServices;

public class Configuration : IPluginConfiguration
{
    public XivChatType ChatChannel = XivChatType.Say;
    public int GilBet = 1000000;
    public float HouseCut = 10f;
    public List<PlayerData> PlayerDatas = [];
    public string LogPath = "";
    public List<string> Log = [];

    //everything below is not used in new UI

    // Required property for IPluginConfiguration
    public int Version { get; set; } = 0;

    // Enables or disables the core feature of the CeeLo plugin.
    public bool EnableFeature { get; set; } = true;

    // Configurable maximum number of players in the game.
    public int MaxPlayers { get; set; } = 25;

    // Allows the configuration window to be moved dynamically.
    public bool IsConfigWindowMovable { get; set; } = true;

    // Flags for optional game rules
    public bool HellRulesEnabled { get; set; } = false;      // Enable or disable Hell Rules
    public bool ReverseRulesEnabled { get; set; } = false;   // Enable or disable Reverse Rules
    public bool SixtyNineRulesEnabled { get; set; } = false; // Enable or disable 69 Rules

    // Saves the current configuration using the Dalamud Plugin Interface.
    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }

    // Resets all configurable properties to their default values.
    public void ResetToDefaults()
    {
        EnableFeature = true;
        MaxPlayers = 8;
        IsConfigWindowMovable = true;

        // Reset the optional rules to default (disabled)
        HellRulesEnabled = false;
        ReverseRulesEnabled = false;
        SixtyNineRulesEnabled = false;
    }
}
