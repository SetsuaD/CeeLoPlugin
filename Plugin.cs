using CeeLoPlugin.Logic;
using CeeLoPlugin.UIv2;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.DalamudServices;
using System.Diagnostics.CodeAnalysis;

namespace CeeLoPlugin;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "CeeLoPlugin";

    // Updated plugin version record.
    public const string PluginVersion = "1.3";
    public static string Version => PluginVersion;

    public Configuration Configuration { get; private set; }
    public CeeLoGameLogic GameLogic { get; private set; }

    public WindowSystem WindowSystem;
    public MainWindowV2 MainWindowV2;
    public ChatSender ChatSender;

    public static Plugin Instance = null!;

    public Plugin(IDalamudPluginInterface dalamudPluginInterface)
    {
        Instance = this;
        ECommonsMain.Init(dalamudPluginInterface, this);

        WindowSystem = new();
        // Draw our windows each frame.
        Svc.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        MainWindowV2 = new();
        ChatSender = new();

        // Load or create configuration.
        Configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Initialize game logic.
        GameLogic = new CeeLoGameLogic();

        // Register a chat command to open the main window.
        Svc.Commands.AddHandler("/ceelo", new CommandInfo(delegate { MainWindowV2.IsOpen = true; })
        {
            HelpMessage = "Open the CeeLo plugin main window."
        });

        // When the config UI is requested, open the main window.
        Svc.PluginInterface.UiBuilder.OpenConfigUi += () => MainWindowV2.IsOpen = true;

        // Register our main UI callback. When UiBuilder.OpenMainUi is triggered, OpenMainUi() is called.
        Svc.PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
    }

    /// <summary>
    /// Called when the main UI should be opened.
    /// </summary>
    private void OpenMainUi()
    {
        MainWindowV2.IsOpen = true;
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        Svc.Commands.RemoveHandler("/ceelo");
        // Unregister the OpenMainUi callback to clean up.
        Svc.PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        ECommonsMain.Dispose();
        Instance = null!;
    }
}
