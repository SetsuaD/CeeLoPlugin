using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using CeeLoPlugin.Windows;

namespace CeeLoPlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    public string Name => "CeeLoPlugin";

    public Configuration Configuration { get; private set; }

    private readonly ConfigWindow _configWindow;

    public Plugin()
    {
        // Initialize configuration
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Initialize ConfigWindow
        _configWindow = new ConfigWindow(this);

        CommandManager.AddHandler("/ceelo", new CommandInfo(OnCommand)
        {
            HelpMessage = "Start a CeeLo game or show the configuration window."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUI;
    }

    private void OnCommand(string command, string args)
    {
        OpenConfigUI();
    }

    private void OpenConfigUI()
    {
        _configWindow.Toggle();
    }

    private void DrawUI()
    {
        _configWindow.Draw();
    }

    public void Dispose()
    {
        CommandManager.RemoveHandler("/ceelo");
        PluginInterface.UiBuilder.Draw -= DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUI;
    }
}
