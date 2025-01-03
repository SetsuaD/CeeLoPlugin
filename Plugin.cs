using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using CeeLoPlugin.Windows;
using CeeLoPlugin.Logic;
using Dalamud.Game.Gui; // Make sure this is here

namespace CeeLoPlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!; // Make sure this is available

    public string Name => "CeeLoPlugin";

    public Configuration Configuration { get; private set; }
    public CeeLoGameLogic GameLogic { get; private set; }

    private readonly MainWindow _mainWindow;
    private readonly ConfigWindow _configWindow;
    private readonly BetWindow _betWindow; // Instantiate BetWindow here

    public Plugin()
    {
        // Initialize configuration
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Initialize game logic
        GameLogic = new CeeLoGameLogic();

        // Initialize windows
        _mainWindow = new MainWindow(this);
        _configWindow = new ConfigWindow(this);
        _betWindow = new BetWindow(ChatGui); // Pass ChatGui to BetWindow constructor

        // Register the /ceelo command
        CommandManager.AddHandler("/ceelo", new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the CeeLo plugin main window."
        });

        // Register UI event handlers
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUI;
        PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUI;
    }

    private void OnCommand(string command, string args)
    {
        OpenMainUI();
    }

    public void OpenMainUI()
    {
        _mainWindow.Toggle();
    }

    public void OpenConfigUI()
    {
        _configWindow.Toggle();
    }

    private void DrawUI()
    {
        _mainWindow.Draw();
        _configWindow.Draw();
        _betWindow.Draw(); // Draw BetWindow here
    }

    public void Dispose()
    {
        CommandManager.RemoveHandler("/ceelo");
        PluginInterface.UiBuilder.Draw -= DrawUI;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUI;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUI;
    }
}
