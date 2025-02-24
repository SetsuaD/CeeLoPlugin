using Dalamud.Game.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeeLoPlugin.UIv2;
public static class Utils
{
    //Which chat types we're allowing to use and their command prefixes so we can use them when it's time to send messages
    public static Dictionary<XivChatType, string> AllowedChatTypes = new()
    {
        [XivChatType.Say] = "/say",
        [XivChatType.Yell] = "/yell",
        [XivChatType.Shout] = "/shout",
        [XivChatType.Party] = "/party",
        [XivChatType.Alliance] = "/alliance",
    };
}
