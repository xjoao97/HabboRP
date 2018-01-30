using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// HtmlPageWebEvent class.
    /// </summary>
    class HtmlPageWebEvent : IWebEvent
    {
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            string Action = Data.Split(',')[0].Split(':')[1];
            string Page = Data.Split(',')[1].Split(':')[1];

            Socket.Send("compose_htmlpage:" + Page + ",action:" + Action);
        }
    }
}
