using Dalamud.Game.Text;
using ECommons.Automation;
using ECommons.Automation.NeoTaskManager;
using ECommons.Throttlers;
using Player = ECommons.GameHelpers.Player;

namespace CeeLoPlugin.UIv2;
public sealed class ChatSender : IDisposable
{
    private TaskManager TaskManager = new(new(abortOnTimeout: true, showDebug: true));

    public void EnqueueMessage(string message) => EnqueueMessage(Plugin.Instance.Configuration.ChatChannel, message);

    public void EnqueueMessage(XivChatType type, string message)
    {
        if(Player.Available)
        {
            TaskManager.Enqueue(() =>
            {
                var cmd = Utils.AllowedChatTypes[type];
                if(Chat.Instance.SanitiseText(message) == message)
                {
                    if(EzThrottler.Throttle("SendMessage", 1200))
                    {
                        Chat.Instance.ExecuteCommand($"{cmd} {message}");
                        return true;
                    }
                }
                else
                {
                    throw new InvalidDataException($"Message contains invalid symbols");
                }
                return false;
            });
        }
    }

    public void Dispose()
    {
        TaskManager.Dispose();
    }
}
